# NAS Test Deployment

Use the [NAS manual](nas-manual.md). This page is retained as a short redirect for the test workflow.

The workflow is manual and runs on the NAS only when a usable development/test version exists. Clone or pull the selected repository version on the NAS, prepare NAS-local ignored configuration and disposable roots, run the documented migration command, and start the base Compose file there. No SSH, `scp`, WSL-to-NAS transfer, image archive, registry, or deployment script is used.

The NAS PostgreSQL instance is external development PostgreSQL. The project does not deploy a PostgreSQL container or volume. Never use the real music library for this test.
