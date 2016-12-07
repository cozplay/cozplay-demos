## Project Description

_Nice to Meet You_ is an experience where Cozmo looks for a new face and tries to befriend the person. Once done, he uses text messaging to maintain contact and continue the relationship.

## YouTube video

https://youtu.be/IeQAT0WpIC8

## Implementation Details [IMPORTANT]

**PART I:**
Use `flask` to handle incoming web requests. The server is exposed to the internet via an `ngrok` tunnel.

How to set it up:

1. Cozmo_response is the method that is designated by flask to handle the incoming requests. No changes required in the code for this.

2. Download ngrok from the official website: https://ngrok.com/

3. Flask sets up the server on port 5000. To have ngrok set up the tunnel, enter the following command: `ngrok http 5000`
4. The console will now provide details of the tunell URLs and will display incoming requests. Keep this window alive:
![Ngrok Console](/nice-to-meet-you/readme_img/ngrok_console.png?raw=true "Ngrok Console")
5. The details of the tunnel URLs with a more verbose debug will be available at the following url: http://127.0.0.1:4040/
6. Copy one of the URLs (in this example: http://330b43d6.ngrok.io/) to be used for the next step
![Ngrok](/nice-to-meet-you/readme_img/ngrok.png?raw=true "Ngrok")

**PART II:**	
A `twilio` account is used to set up a phone number for Cozmo to send messages via SMS/MMS.

How to set it up:

1. Sign up for a Twilio account: https://www.twilio.com/

2. You will receive your Account SID and Authorization Token that will be used for all interactions with the API.
![Twilio](/nice-to-meet-you/readme_img/twilio.png?raw=true "Twilio")

3. Go to the Phone numbers section and choose a number to be associated with the account. You are allowed one free number with a trial account.
4. Once you have a number assigned to the account, select it by going to Phone Numbers -> Manage Numbers -> Active Numbers. Under messaging, change ‘A message comes in’ to Webhook and the URL to the ngrok URL as copied in step 7 of PART I.
![Twilio Config](/nice-to-meet-you/readme_img/twilio_config.png?raw=true "Twilio Config")

5. Detailed information about Twilio package for Python can be found at: https://www.twilio.com/docs/quickstart/python

## Instructions [IMPORTANT]

The modules required in addition to the `Cozmo` module are:

* Flask
* Twilio
* Pillow
* Common


Common is a package included in the Git repo: https://github.com/cozplay/cozplay-demos/tree/master/common

The other modules can be installed via pip if not already present:
`pip install Flask`
`pip install Pillow`
`pip install twilio`

1. Robot volume can be adjusted via the global variable `ROBOT_VOLUME` at the start of the script.
2. If want to remove saved faces from Cozmo’s memory set `ERASE_FACES` to `True`.
3. Set up the following Twilio details in the script:
> Account SID in **line 57**
> Auth token in **line 58**
> Active phone number in **line 60**
4. Set the font in **line 193**. You can place the desired font in the fonts folder. We were using Avenir but it’s not included due to licensing issues.
5. Set the media(picture, video) you want Cozmo to send in **line 242**.
6. You can also change the image in **line 274**.

**Sequence of events:**

1. Cozmo introduces himself and looks for a face.
2. If he recognizes the face, he greets the guest otherwise request their name which must be entered via the console window.
3. Cozmo looks for the face again, points toward it and then requests a cube. 
4. Hold the cube in front of Cozmo, about a couple of inches above the ground, then place it in front of Cozmo once he acknowledges it and the cube starts flashing.
5. Cozmo will pick up the cube and celebrate.
6. Cozmo gives out his phone number and requests you to send a message.
7. After receiving a reply, cozmo sends an MMS.
8. After a few moments he sends another message asking whether you liked it or not.
9. Replying ‘No’ or otherwise deceides Cozmo’s reaction and the experience ends.

## Thoughts for the Future

1. Since this was a one week prototype, not much attention was paid to the story script. Cozmo’s interactions made people chuckle but they weren’t very logical. More thought can be put in this area.
2. Certain sequences like Cozmo acting embarrassed/shy were tough to implement. These were achieved through a combination of default animations, turning and head movement. These ‘hacks’ were not convincing enough to accurately portray something complex like flirtiness. 
3. The above limitation also led to a lot of hard coding which has led to ugly and non reusable code.
4. Using Cozmo’s screen for pictures/animations other than his eyes takes away his emotiveness and should not be used for extended periods.
5. Threading helped to alter the sequential mode of the prototypes before this. However, due to the implementation of the python asynchronous model, it’s still not possible to handle real time input while Cozmo is animating or have Cozmo take on multiple behaviors without breaking the flow of the game in some way.