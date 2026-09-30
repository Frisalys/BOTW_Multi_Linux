#!/usr/bin/env python3
"""
BOTW Multiplayer 1.0.4 - UPDATE protocol probe

Laboratory client. It performs:

    CONNECT
      -> ConnectResponseDTO
    UPDATE
      -> length-prefixed binary ServerDTO
    DISCONNECT

The UPDATE payload uses zero/default values for ClientDTO:

    WorldDTO
    ClientPlayerDTO
    EnemyDTO (empty list)
    QuestsDTO (empty list)

This is intentionally NOT a real Cemu client. Its purpose is to validate
the server's binary serializer/deserializer and discover the ServerDTO
response format.
"""

from __future__ import annotations

import argparse
import json
import socket
import struct
import sys


PACKET_SIZE = 6144
DEFAULT_PORT = 5050
DEFAULT_TIMEOUT = 5.0

PING = 0x01
CONNECT = 0x02
UPDATE = 0x03
DISCONNECT = 0x04


def u8(value: int) -> bytes:
    return struct.pack("<B", value)


def i16(value: int = 0) -> bytes:
    return struct.pack("<h", value)


def i32(value: int = 0) -> bytes:
    return struct.pack("<i", value)


def f32(value: float = 0.0) -> bytes:
    return struct.pack("<f", value)


def boolean(value: bool = False) -> bytes:
    return u8(1 if value else 0)


def vec3(x: float = 0.0, y: float = 0.0, z: float = 0.0) -> bytes:
    return f32(x) + f32(y) + f32(z)


def quaternion() -> bytes:
    return f32(0.0) * 4


def equipment() -> bytes:
    # CharacterEquipment:
    # byte WType + six little-endian shorts.
    return u8(0) + i16(0) * 6


def location() -> bytes:
    # CharacterLocation:
    # byte Map + byte Section.
    return u8(0) + u8(0)


def player_data() -> bytes:
    """
    ClientPlayerDTO, in reflection/field declaration order.
    """
    return b"".join(
        [
            vec3(),
            quaternion(),
            quaternion(),
            quaternion(),
            quaternion(),
            i32(0),       # Animation
            i32(0),       # Health
            f32(0.0),     # AtkUp
            boolean(),    # IsEquipped
            equipment(),
            location(),
            vec3(),       # Bomb
            vec3(),       # Bomb2
            vec3(),       # BombCube
            vec3(),       # BombCube2
        ]
    )


def build_update_payload() -> bytes:
    """
    ClientDTO:

        WorldDTO
        ClientPlayerDTO
        EnemyDTO (List<EnemyData>)
        QuestsDTO (List<string>)

    Lists are encoded as a one-byte element count.
    """
    world = b"".join(
        [
            f32(0.0),     # Time
            i32(0),       # Day
            i32(0),       # Weather
        ]
    )

    enemies = u8(0)
    quests = u8(0)

    return world + player_data() + enemies + quests


def build_packet(message_type: int, payload: bytes = b"") -> bytes:
    packet = bytes([message_type]) + payload

    if len(packet) > PACKET_SIZE:
        raise ValueError(
            f"Packet is {len(packet)} bytes, larger than {PACKET_SIZE}."
        )

    return packet.ljust(PACKET_SIZE, b"\x00")


def recv_exact(sock: socket.socket, size: int) -> bytes:
    data = bytearray()

    while len(data) < size:
        chunk = sock.recv(size - len(data))

        if not chunk:
            raise ConnectionError(
                f"Connection closed while receiving {size} bytes "
                f"(got {len(data)})."
            )

        data.extend(chunk)

    return bytes(data)


def recv_json(sock: socket.socket) -> tuple[bytes, dict]:
    chunks = []
    buffer = b""

    while True:
        chunk = sock.recv(4096)

        if not chunk:
            raise ConnectionError("Server closed before JSON response.")

        chunks.append(chunk)
        buffer += chunk

        try:
            data = json.loads(buffer.decode("utf-8"))
            return b"".join(chunks), data
        except (UnicodeDecodeError, json.JSONDecodeError):
            if len(buffer) > 1024 * 1024:
                raise RuntimeError("JSON response exceeded safety limit.")


def recv_server_packet(sock: socket.socket) -> tuple[bytes, bytes]:
    """
    Server UPDATE response:

        int16 little-endian payload length
        payload

    We first read exactly two bytes, then exactly the advertised payload.
    """
    prefix = recv_exact(sock, 2)
    payload_length = struct.unpack("<h", prefix)[0]

    if payload_length < 0:
        raise RuntimeError(
            f"Invalid negative ServerDTO length: {payload_length}"
        )

    if payload_length > 1024 * 1024:
        raise RuntimeError(
            f"Suspicious ServerDTO length: {payload_length}"
        )

    payload = recv_exact(sock, payload_length)
    return prefix, payload


def hex_preview(data: bytes, limit: int = 128) -> str:
    shown = data[:limit]
    suffix = " ..." if len(data) > limit else ""
    return shown.hex(" ") + suffix


def connect(
    host: str,
    port: int,
    name: str,
    password: str,
    timeout: float,
) -> dict:
    name_bytes = name.encode("utf-8")
    password_bytes = password.encode("utf-8")

    # JSONBuilder encodes string lengths using .NET string.Length.
    if len(name) > 255 or len(password) > 255:
        raise ValueError("Name/password must be <=255 characters.")

    connect_payload = (
        u8(len(name))
        + name_bytes
        + u8(len(password))
        + password_bytes
    )

    packet = build_packet(CONNECT, connect_payload)

    print(f"[+] Connecting to {host}:{port}")
    print(f"[>] CONNECT: {len(packet)} bytes")
    print(f"    Name: {name!r}")
    print(f"    Password bytes: {len(password_bytes)}")

    sock = socket.create_connection((host, port), timeout=timeout)
    sock.settimeout(timeout)

    sock.sendall(packet)

    raw, response = recv_json(sock)

    print(f"[<] CONNECT response: {len(raw)} bytes")
    print(json.dumps(response, indent=2, ensure_ascii=False))

    if response.get("Response") != 1:
        sock.close()
        raise RuntimeError(
            f"CONNECT rejected with Response={response.get('Response')}"
        )

    print(f"[+] Assigned PlayerNumber={response.get('PlayerNumber')}")
    return sock


def main() -> int:
    parser = argparse.ArgumentParser(
        description="Probe BOTW Multiplayer 1.0.4 UPDATE."
    )
    parser.add_argument("host")
    parser.add_argument("--port", type=int, default=DEFAULT_PORT)
    parser.add_argument("--name", default="ZorinUpdateProbe")
    parser.add_argument("--password", default="")
    parser.add_argument("--timeout", type=float, default=DEFAULT_TIMEOUT)
    parser.add_argument(
        "--save-response",
        metavar="FILE",
        help="Save raw ServerDTO payload to a file.",
    )
    args = parser.parse_args()

    try:
        sock = connect(
            args.host,
            args.port,
            args.name,
            args.password,
            args.timeout,
        )

        update_payload = build_update_payload()
        update_packet = build_packet(UPDATE, update_payload)

        print()
        print("=== UPDATE ===")
        print(f"[>] ClientDTO payload: {len(update_payload)} bytes")
        print(f"[>] UPDATE packet: {len(update_packet)} bytes")

        sock.sendall(update_packet)

        prefix, response = recv_server_packet(sock)

        advertised = struct.unpack("<h", prefix)[0]

        print(f"[<] ServerDTO length prefix: {advertised} bytes")
        print(f"[<] ServerDTO payload received: {len(response)} bytes")
        print(f"[<] First bytes: {hex_preview(response)}")

        if args.save_response:
            with open(args.save_response, "wb") as file:
                file.write(prefix + response)
            print(f"[+] Saved raw response to {args.save_response}")

        print()
        print("=== DISCONNECT ===")
        sock.sendall(build_packet(DISCONNECT))
        print("[>] DISCONNECT sent.")

        sock.close()
        print("[+] Probe finished.")
        return 0

    except socket.timeout:
        print("[!] Timed out.", file=sys.stderr)
        return 2
    except ConnectionRefusedError:
        print("[!] Connection refused.", file=sys.stderr)
        return 3
    except (OSError, ConnectionError) as exc:
        print(f"[!] Network error: {exc}", file=sys.stderr)
        return 4
    except (ValueError, RuntimeError) as exc:
        print(f"[!] Protocol error: {exc}", file=sys.stderr)
        return 5


if __name__ == "__main__":
    raise SystemExit(main())
