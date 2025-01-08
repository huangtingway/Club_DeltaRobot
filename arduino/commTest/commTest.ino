void setup() {
   Serial.begin(9600); 
   pinMode(LED_BUILTIN, OUTPUT);
}
void loop() {
   
    if (Serial.available() > 0) {
        String inByte = Serial.readStringUntil('*');
        
        if(inByte == "1") digitalWrite(LED_BUILTIN, 1);
        else if(inByte == "0") digitalWrite(LED_BUILTIN, 0);

    } 
}