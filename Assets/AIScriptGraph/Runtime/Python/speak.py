import pyttsx3
import sys
import base64

def speak(text):
    engine = pyttsx3.init()
    engine.say(text)
    engine.runAndWait()

def b64_to_string(b64_str):
    # 将Base64编码的字符串解码为bytes
    byte_str = base64.b64decode(b64_str)
    # 将bytes转换为普通字符串
    return byte_str.decode('utf-8')
    
if __name__ == "__main__":
    arg = sys.argv[1]
    decoded_string = b64_to_string(arg)
    speak(decoded_string)