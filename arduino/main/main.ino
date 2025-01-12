int RPin = 3, GPin = 4, BPin = 5;

void setup() {
   Serial.begin(9600); 
   pinMode(LED_BUILTIN, OUTPUT);
   pinMode(RPin, OUTPUT);
   pinMode(GPin, OUTPUT);
   pinMode(BPin, OUTPUT);
   
}

void loop() {
   
    if (Serial.available() > 0) {
        String inByte = Serial.readStringUntil('*');
        
        if(inByte == "blink"){
          for(int i = 0;i<3;i++){
            digitalWrite(RPin, 1);
            digitalWrite(GPin, 1);
            digitalWrite(BPin, 1);
            delay(80);
            digitalWrite(RPin, 0);
            digitalWrite(GPin, 0);
            digitalWrite(BPin, 0);
            delay(80);
           }
            
        }
        else if(inByte == "redLight"){
            digitalWrite(RPin, 1);
            digitalWrite(GPin, 0);
            digitalWrite(BPin, 0);
        }
        else if(inByte == "yellowLight"){
            digitalWrite(RPin, 1);
            digitalWrite(GPin, 1);
            digitalWrite(BPin, 0);
        }
        else if(inByte == "greenLight"){
            digitalWrite(RPin, 0);
            digitalWrite(GPin, 1);
            digitalWrite(BPin, 0);
        }
    } 
}
