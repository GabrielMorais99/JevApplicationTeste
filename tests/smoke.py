"""Testes HTTP locais: sem chave, sem chamadas à TypeSafe. Requer build Release."""
import json
import os
import subprocess
import time
import urllib.error
import urllib.request
from pathlib import Path

root = Path(__file__).resolve().parents[1]
api = root / "src/JevApplicationTeste.Api"
env = dict(os.environ, ASPNETCORE_URLS="http://127.0.0.1:5089",
           ASPNETCORE_ENVIRONMENT="Production", Jev__UseMock="true")
process = subprocess.Popen(["dotnet", str(api / "bin/Release/net10.0/JevApplicationTeste.Api.dll")],
                           cwd=api, env=env, stdout=subprocess.DEVNULL)

def request(path, data=None):
    req = urllib.request.Request("http://127.0.0.1:5089" + path,
        data=None if data is None else json.dumps(data).encode(),
        headers={"Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(req, timeout=3) as response:
            return response.status, json.load(response)
    except urllib.error.HTTPError as error:
        return error.code, json.load(error)

try:
    for attempt in range(40):
        try:
            assert request("/health")[0] == 200
            break
        except urllib.error.URLError:
            if process.poll() is not None:
                raise RuntimeError("API encerrou antes de iniciar")
            time.sleep(0.25)
    else:
        raise RuntimeError("API não iniciou em 10 segundos")
    status, result = request("/chamados/classificar", {"descricao": "Integração parou e bloqueou a operação."})
    assert status == 200, result
    assert result["modo"] == "simulado"
    assert result["requerRevisaoHumana"] is True
    assert result["equipeSugerida"] == "backend"
    for data in ({}, {"descricao": ""}, {"descricao": " " * 12}, {"descricao": "x" * 5001}):
        assert request("/chamados/classificar", data)[0] == 400, data
    print("OK: classificação simulada, revisão humana e validação HTTP.")
finally:
    process.terminate()
    try:
        process.wait(timeout=5)
    except subprocess.TimeoutExpired:
        process.kill()
        process.wait()
