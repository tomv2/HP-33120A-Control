# HP 33120A Control Guide and Software
This repository contains a guide and the software for controlling a HP 33120A using Windows 11 with a USB to RS232 Adapter.

## What hardware do I need?
1. HP 33120A with RS232 set to ON. This can be acheieved using the settings menu. Only RS232 or GPIB can be active, so ensure RS232 is selected.
2. RS232 to USB Adapter. Choose this carefully as some don't really work with many RS232 devices. I chose a Plugable brand adapter, which has the Prolific PL2303GT Chipset inside.
3. An RS232 Crossover Adapter (also know as a NULL Adapter). Make sure you order the correct mating type for your RS232 Adapter.
4. Computer with a USB port and correct drivers for your RS232 adapter. Check the manufactures site for the specific drivers, it does vary and sometimes.

## What software do I need?
- I have added my VB .exe here with the source code, so you can just use the .exe. But if you want to make changes, you'll need a code editor and compiler - I used Microsoft Studio.
- You can also use Python to control the device. I have also uploaded some code that will allow you to control the device and this can be integrated into other python scripts for automation. For this you will of course need to download Python (I used version 3.11.3).

## Limitations of the code
Currently, the only thing I haven't got working is the ARB Waveform Generation. This functionality traditional allows the user to generate a waveform using the propriatary software and then send the waveform to the HP 33120A via RS232.
It is worth noting if you are desperate to get this working using a Windows 11 computer, I'd suggest downloading that software as it does still work with Windows 11 - however, it has not been updated in a long time and may not be secure to use. Consider using a VM or an offline computer for this.
