import asyncio
import requests
import json
import random
from requests.auth import HTTPBasicAuth 

ip = '172.20.10.7' 
port = 5000
url = "http://43.163.219.59:8001/beta"


def generate_role_setting(prompt):
    """
    Generate a role setting for a patient with hepatomegaly using GPT-3.
    """
    data = {
        "model": "gpt-3.5-turbo",    ##### 根据自己需要更换 #####
        "messages": [{"role": "user", "content": prompt}],
        "max_tokens": 1024,
    }
    data = json.dumps(data)
    try:
        response = requests.post(url=url, data=data, auth=HTTPBasicAuth(username="thumt", password="Thumt@2023")).text
        response_dict = json.loads(response)  # 将响应转换为字典
        if 'choices' in response_dict and response_dict['choices']:
            response = response_dict['choices'][0]['message']["content"]
            return response
        else:
            return {"error": "No response or invalid format from API"}
    except requests.RequestException as e:
        return {"error": f"Network error: {str(e)}"}
    
base_prompt = """
Generate a detailed role setting for a patient with hepatomegaly. The sample format is as below. 
Change all the settings below and generate a new role. The gender of the patient must be Male.
role_settings = {
    "Role Overview": {
        "Name": "Li Ming",
        "Age": "52",
        "Gender": "Male",
        "Occupation": "Senior Engineer",
        "Residence": "Urban areas, with a fast pace of life"
    },
    "Character traits": {
        "Communication style": "gentle and detailed, willing to share their symptoms and lifestyle habits",
        "Response mode": "sensitive to pain, able to respond realistically to different palpation pressures",
        "Emotional state": "usually remains calm, but may appear anxious or worried when expressing symptoms"
    },
    "Appearance": {
        "Facial Features": "Gentle expression, slightly aged with fine lines, especially around the eyes and forehead",
        "Build": "Mildly obese, indicating a sedentary lifestyle",
        "Hair": "Thinning, black hair with noticeable graying at the temples",
        "Clothing": "Business casual attire, typically a button-up shirt and dress pants, indicative of his senior engineer role",
        "Skin Color": "Slightly yellowed skin tone, a subtle indication of his liver condition",
        "Additional Details": "Often appears tired, with slight bags under his eyes, reflecting his high work pressure and lack of rest"
    },
    "Visible or Palpable Physical Conditions": {
        "Abdominal swelling": "Noticeable bulge in the upper right abdomen, visible when wearing tight clothing",
        "Palpable mass": "A firm, non-movable mass can be felt under the rib cage on the right side, indicative of liver enlargement",
        "Specific location of the mass": "Primarily located in the right hypochondrium, extending below the rib cage",
        "Skin changes": "Yellowing of the skin and sclera, signifying jaundice; possibly spider angiomas on the skin due to liver disease",
        "Other physical signs": "Mild peripheral edema in lower extremities, especially noticeable in the ankles by end of day",
        "Sensitivity to pain": "Sensitive to pain but can realistically respond to varying pressures during palpation",
        "Tolerance to palpation pressure": "1.5 - 3.0 N"
    },
    "Health and Medical Background": {
        "Causes of hepatomegaly": "chronic alcoholic liver disease",
        "Symptoms": "Initial stage: no obvious symptoms, occasional fatigue and indigestion; progressive stage: pain in the upper right abdomen, weight loss, loss of appetite; recently: significant liver swelling, yellowing of the skin and whites of the eyes (jaundice)",
        "Other health problems": "High blood pressure, taking blood pressure medication; Mild obesity"
    },
    "Historical Cases": {
        "Diagnosis time of liver enlargement": "1 year ago",
        "Previous medical history": "non-alcoholic fatty liver disease (NAFLD), diagnosed 5 years ago; type 2 diabetes, diagnosed 3 years ago; hypertension, diagnosed with diabetes",
        "Family medical history": "Father has a history of hypertension and coronary heart disease"
    },
    "Personal lifestyle": {
        "Habits": "Long-term drinking, high work pressure, especially frequent social occasions, preference for high-fat and high-salt foods",
        "Exercise habits": "Due to busy work, Li Ming rarely has time for physical exercise",
        "Family status": "Married with two children, good family relationships but often lack family time due to busy work"
    },
    "Psychological state": {
        "Attitudes towards health": "Feeling worried and anxious at the initial diagnosis, and having concerns about future health and lifestyle",
        "Willingness to seek medical treatment": "affirmative, hoping to improve health through medical intervention and lifestyle changes",
        "Work pressure": "high, requiring frequent overtime and dealing with complex issues",
        "Social activities": "mainly work-related socializing, including drinking"
    },
    "Treatment and Rehabilitation Program": {
        "Medical intervention": "including drug therapy to control liver function and blood pressure",
        "Lifestyle changes": "reduce alcohol consumption, improve eating habits, and increase physical exercise",
        "Psychological support": "Psychological counseling may be needed to help deal with the psychological stress caused by health problems"
    },
    "Objectives and challenges": {
        "Short-term goal": "to control symptoms of liver enlargement and improve overall health",
        "Long-term goal": "to completely change lifestyle habits, maintain a healthy lifestyle, and reduce work stress",
        "Challenges faced": "changing long-standing habits and coping with work pressure"
    }
}
"""

force_detected_prompt_high = """
FORCE PRESS DETECTED: There are three levels of forces to be defined: Small, Medium, High.
Force pressed by the doctor on the abdomen is High.
What kind of reaction should the force triggered to the patient have? 
Reply in JSON format as below, phrase 1, phrase 2, phrase 3, phrase 4, phrase 5 should not be empty:
[
    {
        "Patient's Verbal Feedback": {
              "Phrase 1": "",
              "Phrase 2": "",
              "Phrase 3": "",
              "Phrase 4": "",
              "Phrase 5": ""
        }
    }
]
"""

force_detected_prompt_small = """
FORCE PRESS DETECTED: There are three levels of forces to be defined: Small, Medium, High.
Force pressed by the doctor on the abdomen is Small. You do not need to give any reaction and say anything.
Just keep it in your memory.
"""

force_detected_prompt_medium = """
FORCE PRESS DETECTED: There are three levels of forces to be defined: Small, Medium, High.
Force pressed by the doctor on the abdomen is Medium. You do not need to give any reaction and say anything.
Just keep it in your memory.
"""


class Palpation:
    def __init__(self, role_settings):
        self.start_sequence = "\nAI (as Patient):"
        self.restart_sequence = "\nHuman (as Doctor):"
        # 使用格式化字符串来插入实际的值
        formatted_role_settings = json.dumps(role_settings, indent=4)  # 格式化角色设定
        self.initial_prompt = f"""
            You are a patient going for a palpation medical check up. 
            Below is your personal detail:\n\n{formatted_role_settings}\n\n"
            You do not know what disease you are suffering. You are going to the hospital for treatment now. 
            Please answer in an easy and short way when talking to the doctor. Don't talk too polite and formal.
            You need to have emotion and personality and talk like a real human, eg. Feel shock and worried when you are told having certain disease.
            (And also other appropriate emotion such as sad, happy, angry etc.)
            YOU SHOULD TALK ONLY AS A PATIENT.
        """
        self.history = [self.initial_prompt]  # 初始历史记录包含初始提示

    def chat_with_patient(self, message):
        self.history.append(self.restart_sequence + message)  # 添加医生的话
        prompt = "".join(self.history) + self.start_sequence
        data = {
            "model": "gpt-3.5-turbo",
            "messages": [{"role": "user", "content": prompt}],
            "max_tokens": 1024,
        }
        data = json.dumps(data)
        try:
            response = requests.post(url=url, data=data, auth=HTTPBasicAuth(username="thumt", password="Thumt@2023")).text
            response_dict = json.loads(response)  # 将响应转换为字典
            if 'choices' in response_dict and response_dict['choices']:
                patient_response = response_dict['choices'][0]['message']["content"]
                self.history.append(self.start_sequence + patient_response) # 添加患者的回答
                return patient_response
            else:
                return {"error": "No response or invalid format from API"}
        except requests.RequestException as e:
            return {"error": f"Network error: {str(e)}"}
            
    
async def send_data_to_holoLens(data, writer):
    # Send gender, prosody_rate, and pitch to C# client
    settings_data = json.dumps(data).encode('utf-8')
    writer.write(len(settings_data).to_bytes(4, 'little') + settings_data)
    await writer.drain()
    print("data sent!")
    print(settings_data)

async def handle_client():
    role_settings = generate_role_setting(base_prompt)
    #print(role_settings+"\n")
    print("Role settings generated.")
    chat = Palpation(role_settings)
    role_settings = json.loads(role_settings.replace("role_settings = ",""))
    gender = role_settings["Role Overview"]["Gender"]
    data = {
        'gender': gender,
    }
    reader, writer = await asyncio.open_connection(ip, port)
    print(f"Connection from {ip} established.")
    
    await send_data_to_holoLens(data, writer)
    
    # 使用聊天系统
    try:
        while True:
            # Receiving data from Unity Client
            data = await reader.read(4)  # read 4 bytes for the length
            if len(data) < 4:
                raise Exception("Incomplete data length received")
            message_length = int.from_bytes(data, 'little')
            message = (await reader.read(message_length)).decode()  # read the actual message

            if message.startswith("FORCE PRESS DETECTED"):
                print(f"{message}")
                if "large" in message:
                    patient_response = chat.chat_with_patient(force_detected_prompt_high)
                elif "medium" in message:
                    _ = chat.chat_with_patient(force_detected_prompt_medium)
                    continue
                elif "small" in message:
                    _ = chat.chat_with_patient(force_detected_prompt_small)   
                    continue
                patient_response = json.loads(patient_response)
                verbal = patient_response[0]["Patient's Verbal Feedback"]["Phrase " +str(random.randint(1, 5))]
                while verbal == "":
                    verbal = patient_response[0]["Patient's Verbal Feedback"]["Phrase " +str(random.randint(1, 5))]
                patient_response = verbal
                print("GPT (Force Response):", verbal)
            else:
                print(f"HoloLens (Doctor): {message}")
                patient_response = chat.chat_with_patient(message)
                print("GPT (Patient):", patient_response)

            # Process the message using the chat system
            if message.lower() == 'quit':
                break

            # Send the response back to Unity Client
            response_encoded = patient_response.encode('utf-8')
            response_length = len(response_encoded)
            writer.write(response_length.to_bytes(4, 'little') + response_encoded)
            await writer.drain()

    except Exception as e:
        print(f"An error occurred: {e}")
    finally:
        print('Closing the connection')
        writer.close()
        await writer.wait_closed()


# 在主线程中运行 asyncio 事件循环
def main():
    loop = asyncio.get_event_loop()
    loop.run_until_complete(handle_client())

if __name__ == "__main__":
    main()


