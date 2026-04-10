# What is this?
This folder contains decompressed versions of any masterbundles or legacy bundles
accessed by the **Unturned Data File** VS Code extension. Any files in here can be safely deleted.

Some decompressed bundles can use quite a bit of storage, so if this becomes a problem this folder can be moved, or interacting with bundles can be turned off completely, through the extension settings.


# File Format
File paths are hashed into a 32-bit binary string then given an index within their hash that increments each time a hash-collision occurs.
The `.toc` (table-of-contents) files keep track of absolute paths of bundles to unpacked bundle paths.

The first 8 bytes keep track of the next index. From the 9th byte and onwards, each line contains the path to the bundle, a NUL character, and a path to the unpacked bundle, followed by a newline.

Text is encoded using UTF-8.