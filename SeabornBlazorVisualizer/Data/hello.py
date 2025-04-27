import clr
from System import DateTime

def get_hello_world():
    # Get the current time
    current_time = DateTime.Now;    
    # Concatenate with a string
    message = f"The current time is: {current_time}"
    return message;