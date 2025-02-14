# Use the official PowerShell image as a base
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview-bookworm-slim

# Create the user
RUN useradd -m -s /bin/bash -u 1000 vscode \
    && groupadd docker \
    && usermod -aG docker vscode

# [Optional] Uncomment this section to install additional packages.
# RUN apt-get update && export DEBIAN_FRONTEND=noninteractive \
#     && apt-get -y install --no-install-recommends <your-package-list-here>

# Set the user
USER vscode