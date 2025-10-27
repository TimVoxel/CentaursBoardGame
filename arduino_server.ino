#define BAUD 9600

#define MOTOR_1_START 2
#define MOTOR_2_START 9

#define MIN_DELAY 2
#define MAX_STEP 4

#define MOTOR_1 0 
#define MOTOR_2 1
#define RING_2 2
#define RING_3 3

#define SECTOR_COUNT 24
#define STEPS_PER_ROTATION 2048

#define ROTATE_CODE "r"

uint16_t stepsLeft1 = 0;
uint16_t stepsLeft2 = 0;

uint8_t step1 = 0;
uint8_t step2 = 0;
uint8_t rotateDelayMillis = max(MIN_DELAY, 2);

enum class State : uint8_t
{
  AwaitingInput,
  Rotating
};

State state = State::AwaitingInput;

uint16_t convertSectorsToSteps(uint8_t sectorCount)
{
  return (STEPS_PER_ROTATION / SECTOR_COUNT) * sectorCount;
}

void setup() {

  long seed = analogRead(A0);
  seed ^= analogRead(A1) << 10;
  randomSeed(seed);
  
  Serial.begin(BAUD);

  for (uint8_t i = 0; i < MAX_STEP; i++)
  {
    pinMode(MOTOR_1_START + i, OUTPUT);
    pinMode(MOTOR_2_START + i, OUTPUT);
  }

  pinMode(LED_BUILTIN, OUTPUT);
}

void loop() 
{
  handleRequests();

  if (stepsLeft1 > 0 || stepsLeft2 > 0)
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
    String args = input.substring(sepIndex + 1);

    unsigned int startIndex = 0;

    if (action == ROTATE_CODE) 
    {
      while (true)
      {
        uint16_t spaceIndex = args.indexOf(' ', startIndex);
        uint16_t commaIndex = args.indexOf(',', startIndex);

        if (spaceIndex == -1)
        {
          break;
        }

        uint8_t ringIndex = args.substring(startIndex, spaceIndex).toInt();
        uint8_t sectorCount = args.substring(spaceIndex + 1, commaIndex).toInt();

        startRotatingRing(ringIndex, sectorCount);

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
  if (stepsLeft1 > 0)
  {
    performStep(MOTOR_1, true);
    stepsLeft1--;
  }
      
  if (stepsLeft2 > 0)
  {
    performStep(MOTOR_2, true);
    stepsLeft2--;
  }

  if (stepsLeft1 <= 0 && stepsLeft2 <= 0)
  {
    sendSuccessResponse("Rotated rings successfully");
  }
  delay(rotateDelayMillis);
}

void startRotatingRing(uint8_t ring, uint8_t sectorCount) {
    if (ring == RING_2)
    {
      stepsLeft1 = convertSectorsToSteps(sectorCount);
    }
    else if (ring == RING_3)
    {
      stepsLeft2 = convertSectorsToSteps(sectorCount);
    }

    state = State::Rotating;
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
  if (motor == MOTOR_1)
  {
    if (clockwise)
    {
      sendStepData(MOTOR_1_START, step1);
    }
    else 
    {
      sendStepData(MOTOR_1_START, MAX_STEP - step1);
    }
    
    step1++;

    if (step1 == MAX_STEP)
    {
      step1 = 0;
    }
  }
  else if (motor == MOTOR_2)
  {
    if (clockwise)
    {
      sendStepData(MOTOR_2_START, step2);
    }
    else 
    {
      sendStepData(MOTOR_2_START, MAX_STEP - step2);
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