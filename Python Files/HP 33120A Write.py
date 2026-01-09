import serial
import serial.tools.list_ports

# ----------------------------
# Auto-detect PL2303 USB-serial
# ----------------------------
def detect_pl2303():
    ports = list(serial.tools.list_ports.comports())
    pl_ports = [p.device for p in ports if "PL2303" in p.description]
    if not pl_ports:
        print("No PL2303 USB-serial adapter detected!")
        return None
    print("Detected PL2303 on:", pl_ports[0])
    return pl_ports[0]

# ----------------------------
# Send SCPI command
# ----------------------------
def send_command(ser, command):
    """Send a SCPI command and return response (if any)"""
    ser.write((command + '\r\n').encode())
    try:
        response = ser.readline().decode().strip()
        if response:
            return response
    except:
        return None

# ----------------------------
# Query current waveform settings
# ----------------------------
def query_current_settings(ser):
    waveform = send_command(ser, "FUNC?")
    freq = send_command(ser, "FREQ?")
    volt = send_command(ser, "VOLT?")
    offset = send_command(ser, "VOLT:OFFS?")
    duty = None
    if waveform == "SQU":
        duty = send_command(ser, "PULSe:DCYCle?")
    print("\n--- Current waveform configuration ---")
    print("Function:        ", waveform)
    print("Frequency (Hz):  ", freq)
    print("Amplitude (Vpp): ", volt)
    print("Offset (V):      ", offset)
    if duty:
        print("Duty cycle (%):  ", duty)
    print("-------------------------------------\n")

# ----------------------------
# Main interactive loop
# ----------------------------
def main():
    com_port = detect_pl2303()
    if not com_port:
        return
    
    # Open serial port
    ser = serial.Serial(com_port, baudrate=9600, timeout=1)
    
    # Clear previous errors
    send_command(ser, "*CLS")
    
    print("\nConnected to HP 33120A. Type 'quit' at any prompt to exit.")

    while True:
        # Waveform selection
        waveform = input("\nWaveform (SIN, SQU, TRI, RAMP, NOIS, DC): ").upper()
        if waveform == 'QUIT':
            break
        if waveform not in ['SIN', 'SQU', 'TRI', 'RAMP', 'NOIS', 'DC']:
            print("Invalid waveform type.")
            continue
        
        # Frequency
        freq = input("Frequency (Hz): ")
        if freq.upper() == 'QUIT':
            break
        
        # Amplitude
        volt = input("Amplitude (Vpp): ")
        if volt.upper() == 'QUIT':
            break
        
        # Send waveform, frequency, amplitude
        send_command(ser, f"FUNC {waveform}")
        send_command(ser, f"FREQ {freq}")
        send_command(ser, f"VOLT {volt}")
        
        # If square wave, set duty cycle
        if waveform == 'SQU':
            duty = input("Duty cycle (%) [20-80]: ")
            if duty.upper() == 'QUIT':
                break
            send_command(ser, f"PULSe:DCYCle {duty}")
        
        # Confirm settings by querying the instrument
        query_current_settings(ser)
    
    ser.close()
    print("Disconnected.")

# ----------------------------
if __name__ == "__main__":
    main()
