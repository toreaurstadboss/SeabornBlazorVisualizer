import clr
from System import DateTime

import numpy as np
import pandas as pd

def get_hello_world():
    # Get the current time
    current_time = DateTime.Now;    
    # Concatenate with a string
    message = f"The current time is woha: {current_time}"
    return message;