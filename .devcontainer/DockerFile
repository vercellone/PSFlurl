# Use the official PowerShell image as a base
FROM mcr.microsoft.com/devcontainers/dotnet

# # Install necessary dependencies
# RUN apt-get update && apt-get install -y \
#     curl \
#     git \
#     && apt-get clean \
#     && rm -rf /var/lib/apt/lists/*

# Set the default shell to PowerShell
SHELL ["pwsh", "-Command"]

# Set the working directory
WORKDIR /workspaces/PSFlurl