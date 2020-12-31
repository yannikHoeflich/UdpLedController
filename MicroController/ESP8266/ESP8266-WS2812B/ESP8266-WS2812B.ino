#include <ESP8266WiFi.h>
#include <WiFiUdp.h>
#include "FastLED.h"


//wifi parameters
#define STASSID "WIFI SSID"
#define STAPSK  "WIFI PASSWORD"

//led parameters
#define NUM_LEDS 300
#define DATA_PIN 6


CRGB leds[NUM_LEDS];

const char* ssid = STASSID;
const char* password = STAPSK;
WiFiUDP Udp;
unsigned int localUdpPort = 4210;  // local port to listen on
char incomingPacket[NUM_LEDS * 3 + 1];  // buffer for incoming packets
char  replyPacket[2];  // a reply string to send back


void setup() {
  FastLED.addLeds<NEOPIXEL, DATA_PIN>(leds, NUM_LEDS);
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
  }
  Udp.begin(localUdpPort);
}

void loop() {
  if (WiFi.status() != WL_CONNECTED){
    WiFi.begin(ssid, password);
    while (WiFi.status() != WL_CONNECTED){
      delay(500);
    }
  }
  int packetSize = Udp.parsePacket();
  if (packetSize)
  {
    Udp.read(incomingPacket, sizeof(incomingPacket));

    if(incomingPacket[0] == 0){
      replyPacket[0] = NUM_LEDS;
      replyPacket[1] = NUM_LEDS >> 8;
    } else {
      for(int i = 0; i < NUM_LEDS; i++){
        leds[i] = CRGB(incomingPacket[i * 3 + 1], incomingPacket[i * 3 + 2], incomingPacket[i * 3 + 3]);
      }
      
      replyPacket[0] = 0;
      replyPacket[1] = 0;
      FastLED.show();
    }

    // send back a reply, to the IP address and port we got the packet from
    Udp.beginPacket(Udp.remoteIP(), Udp.remotePort());
    Udp.write(replyPacket);
    Udp.endPacket();
  }
}
