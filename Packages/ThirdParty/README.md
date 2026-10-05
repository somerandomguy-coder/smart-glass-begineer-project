# Local XREAL package

Place the vendor download here as `com.xreal.xr.tgz`, then extract its `package` folder and rename it to `com.xreal.xr`.

The archive and extracted package are intentionally excluded from Git. Download **XREAL SDK for Unity 3.1.0** from the official XREAL developer portal and ensure the filename is exactly `com.xreal.xr.tgz`; Unity Package Manager will then resolve the `com.xreal.xr` dependency declared in `../manifest.json`.

The local package has one editor-only compatibility adjustment: its native callback is started only in an Android player build. The download has Android libraries only, so this keeps the Windows Editor simulator from trying to load a nonexistent Windows `XREALXRPlugin` DLL. Android builds retain the vendor callback.
