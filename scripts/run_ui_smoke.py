from __future__ import annotations

import os
import pathlib
import subprocess
import sys
import time

root = pathlib.Path(__file__).resolve().parents[1]
build_dir = root / "diagnostic-build"
data_dir = root / "ui-smoke-data"
log_dir = root / "ui-smoke-logs"
result = data_dir / "ui-smoke-results.json"
trace = build_dir / "startup-trace.log"
data_dir.mkdir(parents=True, exist_ok=True)
log_dir.mkdir(parents=True, exist_ok=True)
for path in (result, trace):
    if path.exists():
        path.unlink()
exe = build_dir / "CXAudioOutput.exe"
if not exe.is_file():
    raise SystemExit(f"missing executable: {exe}")

stdout_path = log_dir / "stdout.log"
stderr_path = log_dir / "stderr.log"
for path in (stdout_path, stderr_path):
    path.write_text("", encoding="utf-8")

env = os.environ.copy()
env["CXAUDIO_TEST_DATA_DIRECTORY"] = str(data_dir)
env["CXAUDIO_PORT"] = "15240"

with stdout_path.open("w", encoding="utf-8") as stdout, stderr_path.open("w", encoding="utf-8") as stderr:
    process = subprocess.Popen(
        [str(exe), "--ui-smoke-test", "--demo", "--port=15240"],
        cwd=str(build_dir),
        env=env,
        stdin=subprocess.DEVNULL,
        stdout=stdout,
        stderr=stderr,
        close_fds=False,
    )
    started = time.monotonic()
    timeout = 90.0
    while True:
        code = process.poll()
        if code is not None:
            print(f"exit_code={code}")
            break
        if time.monotonic() - started >= timeout:
            process.terminate()
            try:
                code = process.wait(timeout=10)
            except subprocess.TimeoutExpired:
                process.kill()
                code = process.wait(timeout=10)
            print(f"timeout_seconds={timeout:g}")
            print(f"exit_code={code}")
            raise SystemExit(124)
        time.sleep(0.25)

print(f"result_exists={result.is_file()}")
print(f"result={result}")
print(f"trace_exists={trace.is_file()}")
print(f"trace={trace}")
if result.is_file():
    print(result.read_text(encoding="utf-8", errors="replace"))
if trace.is_file():
    print("--- startup trace tail ---")
    lines = trace.read_text(encoding="utf-8", errors="replace").splitlines()
    print("\n".join(lines[-80:]))
if code != 0:
    print("--- stderr ---")
    print(stderr_path.read_text(encoding="utf-8", errors="replace"))
    raise SystemExit(code)
if not result.is_file():
    raise SystemExit("UI smoke process exited without a result file")
try:
    import json
    payload = json.loads(result.read_text(encoding="utf-8"))
except Exception as exc:
    raise SystemExit(f"invalid result json: {exc}")
if payload.get("passed") is not True:
    raise SystemExit("UI smoke reported failure")
