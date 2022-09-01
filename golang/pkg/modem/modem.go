package modem

import (
	"fmt"
	"github.com/tarm/serial"
	"io/ioutil"
	"time"
)

var openPort *serial.Port

var validResponses = []string {
	"OK",
	"ERROR",
	"CONNECT",
	"VCON",
}

func Start(portName string) error {
	// connect via serial
	var err error
	openPort, err = serial.OpenPort(&serial.Config{
		Name:        portName,
		Baud:        2400,
		ReadTimeout: 30,
	})
	if err != nil {
		return err
	}

	buf := make([]byte, 2400)
	for(true) {
		n, err := openPort.Read(buf)
		if err != nil {
			return err
		}

		fmt.Printf("%q\n", buf[:n])
	}

	return nil
}

func startDialTone() error {
	err := sendCommand("AT+FCLASS=8") // Enter voice mode
	if err != nil {
		return err
	}

	err = sendCommand("AT+VLS=1") // Go off-hook
	if err != nil {
		return err
	}

	err = sendCommand("AT+VSM=1,8000") // 8 bit unsigned PCM
	if err != nil {
		return err
	}

	err = sendCommand("AT+VTX") // Voice transmission mode
	return err
}

func stopDialTone() error {
	err := sendCommand(fmt.Sprintf("\000%c%c", rune(0x10), rune(0x03)))
	if err != nil {
		return err
	}

	err = sendEscape()
	if err != nil {
		return err
	}

	err = sendCommand("ATH0") // Go on-hook
	if err != nil {
		return err
	}

	err = reset()
	return err
}

func sendCommand(command string) error {
	dat := fmt.Sprintf("%s\r\n", command)
	_, err := openPort.Write([]byte(dat))
	if err != nil {
		return err
	}

	// TODO: read response

	return nil
}

func send(bytes []byte) error {
	_, err := openPort.Write(bytes)
	if err != nil {
		return err
	}

	return nil
}

func sendEscape() error {
	time.Sleep(1 * time.Second)
	err := send([]byte("+++"))
	time.Sleep(1 * time.Second)

	return err
}

func reset() error {
	err := sendCommand("ATZ0") // send reset command
	if err != nil {
		return err
	}

	return sendCommand("ATE0") // don't echo responses
}

func answer() error {
	// TODO
	return nil
}

func disconnect() error {
	// TODO
	return nil
}

func update() error {
	// TODO
	return nil
}

func readDialToneDat() ([]byte, error) {
	f, err := ioutil.ReadFile("dial-tone.wav")
	if err != nil {
		return nil, err
	}

	// remove 44 bytes (header) and return dial tone data
	return f[44:], nil
}
