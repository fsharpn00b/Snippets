This script lets you write self-referential JSON. That is, you can retrieve values already set in the JSON object and insert them elsewhere.

For example, this JSON:

```json
{
    "a" : "1",
    "b" : "[a]",
    "c" : {
        "d" : "[a]"
    },
    "e" : "[d]",
    "f" : [{
        "g" : "[d]"
    }],
    "h" : "[date]"
}
```

becomes, given an initial dictionary where `date` is set to the current date/time:

```json
{
    "a": "1",
    "b": "1",
    "c": {
        "d": "1"
    },
    "e": "1",
    "f": [
        {
            "g": "1"
        }
    ],
    "h": "20241203_014114"
}
```

I wrote this script to help me create an automated backup program with tasks defined in JSON.

Example usage:

```python
import datetime
import json
import sys
import replace_in_json

def get_date () :
    return datetime.datetime.now().strftime('%Y%m%d_%H%M%S')

with open (sys.argv[1], 'r') as f :
    configuration = json.load (f)
    print (json.dumps (replace_in_json.replace_in_json (configuration, { 'date' : get_date () }), indent=2))
```
