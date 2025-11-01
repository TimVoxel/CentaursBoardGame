#define BAUD 9600

#define MOTOR_MIDDLE 0 
#define MOTOR_INNER 1
#define MOTOR_MIDDLE_START 2
#define MOTOR_INNER_START 9

#define MIN_DELAY 2
#define MAX_STEP 4

#define RING_MIDDLE 2
#define RING_INNER 3

#define SECTOR_COUNT 24
#define STEPS_PER_ROTATION 2048

#define ROTATE_CODE "r"

struct BoardRotation
{
  uint16_t stepsLeft;
  bool isClockwise;
};

const uint8_t driverGearTeethNumber = 20;
const uint8_t middleGearTeethNumber = 147;
const uint8_t innerGearTeethNumber = 100;

const float middleGearFactor = middleGearTeethNumber / ((float) driverGearTeethNumber);
const float innerGearFactor = innerGearTeethNumber / ((float) driverGearTeethNumber);
const float stepsPerSector = ((float) STEPS_PER_ROTATION) / ((float) SECTOR_COUNT);

BoardRotation middleRingRotation;
BoardRotation innerRingRotation;

uint8_t step1 = 0;
uint8_t step2 = 0;

uint16_t convertSectorsToStepsInner(uint8_t sectorCount)
{
  return stepsPerSector * sectorCount * innerGearFactor;
}

uint16_t convertSectorsToStepsMiddle(uint8_t sectorCount)
{
  return stepsPerSector * sectorCount * middleGearFactor;
}

void setup() {

  long seed = analogRead(A0);
  seed ^= analogRead(A1) << 10;
  randomSeed(seed);
  
  Serial.begin(BAUD);

  for (uint8_t i = 0; i < MAX_STEP; i++)
  {
    pinMode(MOTOR_MIDDLE_START + i, OUTPUT);
    pinMode(MOTOR_INNER_START + i, OUTPUT);
  }

  pinMode(LED_BUILTIN, OUTPUT);
}

void loop() 
{
  handleRequests();

  if (middleRingRotation.stepsLeft > 0 || innerRingRotation.stepsLeft > 0)
  {
    handleRotations();
  }
}

void handleRequests()
{
  if (Serial.available() == 0) 
  {
    return;
  }

    String input = Serial.readStringUntil('\n');

    uint16_t sepIndex = input.indexOf(':');
    if (sepIndex == -1)
    {
      return;
    } 

    String action = input.substring(0, sepIndex);
    unsigned int startIndex = sepIndex + 1;

    if (action == ROTATE_CODE) 
    {
      while (true)
      {
        uint16_t spaceIndex = input.indexOf(' ', startIndex + 1);
        uint16_t commaIndex = input.indexOf(',', startIndex + 1);

        if (spaceIndex == -1)
        {
          break;
        }

        bool isClockwise = input[startIndex] == '1';
        uint8_t ringIndex = input.substring(startIndex + 1, spaceIndex).toInt();
        uint8_t sectorCount = input.substring(spaceIndex + 1, commaIndex).toInt();

        startRotatingRing(ringIndex, sectorCount, isClockwise);

        if (commaIndex != -1)
        {
          startIndex = commaIndex + 1;
        }
        else 
        {
          break;
        }
      }
    } 
    else
    {
      sendFailureResponse("Unable to parse request");
    }
}

void handleRotations()
{
  if (middleRingRotation.stepsLeft > 0)
  {
    performStep(MOTOR_MIDDLE, middleRingRotation.isClockwise);
    middleRingRotation.stepsLeft--;
  }
      
  if (innerRingRotation.stepsLeft > 0)
  {
    performStep(MOTOR_INNER, innerRingRotation.isClockwise);
    innerRingRotation.stepsLeft--;
  }

  if (middleRingRotation.stepsLeft <= 0 && innerRingRotation.stepsLeft <= 0)
  {
    sendSuccessResponse("Rotated rings successfully");
  }
  delay(MIN_DELAY);
}

void startRotatingRing(uint8_t ring, uint8_t sectorCount, bool isClockwise) {
    if (ring == RING_MIDDLE)
    {
      middleRingRotation = { convertSectorsToStepsMiddle(sectorCount), isClockwise };
    }
    else if (ring == RING_INNER)
    {
      innerRingRotation = { convertSectorsToStepsInner(sectorCount), isClockwise };
    }
}

void sendStepData(uint8_t startPin, uint8_t step)
{
  for (uint8_t i = 0; i < MAX_STEP; i++)
  {
      if (step == i)
      {
        digitalWrite(startPin + i, HIGH);
      }
      else 
      {
        digitalWrite(startPin + i, LOW);
      }
  }
}

void performStep(uint8_t motor, bool clockwise)
{
  if (motor == MOTOR_MIDDLE)
  {
    if (clockwise)
    {
      sendStepData(MOTOR_MIDDLE_START, step1);
    }
    else 
    {
      sendStepData(MOTOR_MIDDLE_START, MAX_STEP - step1);
    }
    
    step1++;

    if (step1 == MAX_STEP)
    {
      step1 = 0;
    }
  }
  else if (motor == MOTOR_INNER)
  {
    if (clockwise)
    {
      sendStepData(MOTOR_INNER_START, step2);
    }
    else 
    {
      sendStepData(MOTOR_INNER_START, MAX_STEP - step2);
    }
    step2++;

    if (step2 == MAX_STEP)
    {
      step2 = 0;
    }
  }
  else 
  {
    sendFailureResponse("No such motor");
  }
}

void sendSuccessResponse(String message)
{
  Serial.print("s:");
  Serial.print(message);
  Serial.print('\0');
}

void sendFailureResponse(String message)
{
  Serial.print("f:");
  Serial.print(message);
  Serial.print('\0');
}