# Enterprise YAML‑Based Testing Framework

A practical, extensible test runner for APIs, RPC and system workflows, configured in YAML. Built to be easy to read, easy to extend, and fast enough for day‑to‑day CI.

[.NET 5+]	[C# 9]	[Plugin‑based]

---

## Why it matters (for teams)
- Define tests in YAML, not code
- Mix protocols in one flow (HTTP, RPC, MBUS, CMD, Scripts)
- Assert results and performance in the same place
- Extend via small plugins rather than changing core

---

## What it does (today)
- YAML‑first test suites with variables, tables, and JPath resolution
- Step executors for HTTP, RPC, MBUS, CMD, and Script
- Rich assertions (functional + performance)
- Per‑step performance metrics exposed in context:
  - `$curStep:performance.executionTimeMs`
  - `$curStep:performance.assertionTimeMs`
  - `$curStep:performance.startTime` / `endTime`
  - `$curStep:performance.stepType`
- Protocol‑specific metrics via `$curStep:performanceData.*` (when provided by the executor)

What it doesn’t claim:
- No built‑in distributed/horizontal runner
- No web dashboard; export metrics and visualize in your preferred tools

---

## 60‑second tour
1) Write a YAML test
<small>(functional + performance in one place)</small>

```yaml
Name: "User API smoke"
Steps:
  - Name: "GET user"
    Type: "http"
    Input:
      Method: "GET"
      RequestUri: "{{$var:api}}/users/123"
    Asserters:
      - AssertEq: { JPathExpr: "$curStep:output.statusCode", ConstExpr: 200 }
      - AssertLt: { JPathExpr: "$curStep:performance.executionTimeMs", ConstExpr: 800 }
```

2) Run the suite (CLI)
- Produces machine‑readable output suitable for CI

3) Act on results
- Fail fast on functional or performance regressions

---

## Highlights
- Clean plugin architecture (Factory, Strategy, Template Method)
- Assertions designed for readability and diagnostics
- Context system for sharing data across steps
- Examples for microservices, data processing, and performance checks

---

## What’s implemented vs. out of scope
- Implemented: single‑process runner, per‑step timings, protocol plugins, YAML tables/repeaters
- Out of scope (for now): distributed execution, cluster scheduling, built‑in dashboards

---

## Explore the docs
- Technical architecture → ./docs/TECHNICAL_ARCHITECTURE.md
- Implementation examples → ./docs/IMPLEMENTATION_EXAMPLES.md
- Performance & scalability (grounded) → ./docs/PERFORMANCE_SCALABILITY.md
- API & extension points → ./docs/API_REFERENCE.md

Examples:
- Basic HTTP → ./examples/basic/simple-api-test.yaml
- Advanced E2E → ./examples/advanced/microservices-integration.yaml

---

## About this project (portfolio context)
This repository showcases design decisions and patterns drawn from real enterprise work, presented without proprietary code. It focuses on clarity, maintainability, and pragmatic engineering over hype.
