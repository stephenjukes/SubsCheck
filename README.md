# Setup
## Download the .NET SDK
.NET SDK is essentially what allows me to write the program that allocates subs to members. _You_ will need it to _use_ the program, and can be installed from [here](https://dotnet.microsoft.com/en-us/download/dotnet).

* Select the latest version with Long Term Support
* You will see a table as below. Select the installer from the OS that corresponds with your device. If you're not sure which installer to use, you can check if your computer is 32-bit (x86) or 64-bit (x64) on Windows, go to Settings > System > About, and look under "Device specifications" for "System type," which will say "64-bit Operating System" or "32-bit Operating System," or "x64-based processor" vs. "x86-based processor". On macOS, check About This Mac for "Intel" (x86) or "Apple M1/M2" (ARM). 

    | OS      | Installers                   | Binaries   | 
    | ------- | ---------------------------- | ---------- |
    | Linux   | Package manager instructions | Arm32, etc |
    | macOS   | Arm64, x64                   | Arm64, x64 |
    | Windows | x64, x86                     | x64, x86   |

* Selecting the installer will download it to your computer. You can either open it up when prompted by the downloader on your screen, or by navigating to the folder that it was downloaded to, (this will be your Downloads folder on Windows).
* Once opened, follow the prompts of the installer, (you should be fine to accept all defaults).
* Check that the .NET SDK has been installed successfully by bringing up the terminal (typing `terminal` in the search bar or similar) and entering `dotnet --list-sdks`. If successfully installed, the SDK version should be displayed.

## Get the code from Github
## Restore nuget packages
## Test run

# Running the tool
## Transactions
## Members
## Config
## Run (as before)

# Checking and manipulating the output


1. download .NET SDK (https://learn.microsoft.com/en-us/dotnet/core/install/)

2. copy .zip file from github (extract to desired folder)

3. in terminal, navigate to the folder containing the .net project

4. (run ‘dotnet restore’ to restore any nuget packages)

5. update update config, members and transactions files

6. (inputs is put into the same folder as the project, which means the path is not found)

7. run ‘dotnet run’ (in the same project you navigated to earlier)