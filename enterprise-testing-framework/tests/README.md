Curated unit tests (sanitized)

- These are selected unit tests copied from an internal project and lightly sanitized (headers removed, identifiers may be anonymized).
- They are included to illustrate testing style, coverage of assertions, context resolution, HTTP executor, performance metrics, and suite loading.
- They are not expected to compile or run standalone in this portfolio repository.

Structure
- tests/unit/src/...  C# NUnit tests demonstrating:
  - Core context behavior (TestContext*)
  - Expression/assertion behavior (Equal/Less/Greater)
  - HTTP step execution assertions
  - Performance metrics access and threshold assertions
  - Test suite file discovery

Note
- Proprietary notices were removed for portfolio use. Replace placeholders or adjust namespaces if you adapt these into your own projects.
