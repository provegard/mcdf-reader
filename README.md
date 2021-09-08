# mcdf-reader

A C# library for reading Microsoft Compound Document files.

## Related work

* [OpenMcdf](https://github.com/ironfede/openmcdf)
* [OpenMcdf-2](https://github.com/CodeCavePro/OpenMCDF)

I found both of these to be buggy, in particular when it came to reading OLE property streams.

## Limitations

Writing/modifying compound document files is not supported.

## References

* [[MS-CFB]: Compound File Binary File Format](https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-cfb/53989ce4-7b05-4f8d-829b-d08d6148375b)

## Development

.NET 6 with C# 10 support is required.

To build, run:

    dotnet build

To run tests, run:

    dotnet test

## Contributions

Contributions are welcome! Please open an issue to discuss the
topic at hand before you open a PR.

## License

Licensed under the MIT license, see [LICENSE](./LICENSE).