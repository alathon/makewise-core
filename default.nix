with import <nixpkgs>{};

stdenv.mkDerivation rec {
  name = "makewise-core";
  version = "0.0.1";
  buildInputs = [
    # dotnet
    dotnet-sdk
    fsharp
    ];
  NUGET_FALLBACK_PACKAGES = "/nuget/fallback";
  NUGET_PACKAGES = "/nuget/packages";
  DOTNET_SKIP_FIRST_TIME_EXPERIENCE = true;
}
