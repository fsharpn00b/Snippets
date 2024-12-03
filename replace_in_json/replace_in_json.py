import functools, json, re

# Walk JSON dictionary (x) and replace "[key]" with the value for key.
def replace_in_json (x, special=None) :
# We thread a master dictionary (d) through all functions, and record all keys and values in it. That lets us look up keys from higher levels of nesting.
# We walk the JSON dictionary depth first.

	pattern = '\\[(\\w+)\\]'

	def dispatch (x, d) :
		if type (x) is dict :
			return replace_in_dictionary (x, d)
		elif type (x) is list :
			return replace_in_list (x, d)
		elif type (x) is str :
			return replace_in_str (x, d), d
		else :
			return x, d

	def replace_in_dictionary (d2, d) :
		def folder (acc, item) :
			key, value = item[0], item[1]
# Transform the value and get the updated master dictionary.
			result, d_ = dispatch (value, acc[1])
# Add the transformed value to the dictionary for the current level of nesting.
			acc[0][key] = result
# Add the transformed value to the master dictionary.
			d_[key] = result
			return acc[0], d_
# The accumulator has two parts. One is an empty dictionary, which will become the transform of d2. The other is the master dictionary.
		return functools.reduce (folder, d2.items (), [dict (), d])

	def replace_in_list (xs, d) :
		def folder (acc, item) :
# Transform the value and get the updated master dictionary.
			result, d_ = dispatch (item, acc[1])
# Add the transformed value to the list for the current level of nesting.
			acc[0].append (result)
			return acc[0], d_
# The accumulator has two parts. One is an empty list, which will become the transform of xs. The other is the master dictionary.
		return functools.reduce (folder, xs, [[], d])

	def replace_in_str (s, d) :
		def lookup (key) :
			if key in d : return d[key]
			elif None != special and key in special : return special[key]
			else : raise Exception ('replace_in_json: Unknown key: ' + key + '.\nDictionary: ' + json.dumps(d) + '\nSpecial: ' + json.dumps(special))
# Do not use re.findall, as it returns strings instead of match objects.
		matches = re.finditer (pattern, s, re.IGNORECASE)
		replacements = map (lambda x : x.group (1), matches)
		return functools.reduce (lambda acc, item : re.sub ('\\[' + item + '\\]', lookup (item), acc), replacements, s)

# Start with an empty master dictionary.
	result = dispatch (x, dict ())
# Discard the master dictionary.
	return result[0]

def test () :
	s = '''
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
	'''
	result = replace_in_json (json.loads (s), { 'date' : 'Today\'s date' })
	print (result)

# Expected output
# {'a': '1', 'b': '1', 'c': {'d': '1'}, 'e': '1', 'f': [{'g': '1'}], 'h': "Today's date"}

def test2 () :
        s = '''
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
                "h" : "[this_key_does_not_exist]"
        }
        '''
        result = replace_in_json (json.loads (s))
# Expected output
# Exception: replace_in_json: Unknown key: this_key_does_not_exist.
# Dictionary: {"a": "1", "b": "1", "d": "1", "c": {"d": "1"}, "e": "1", "g": "1", "f": [{"g": "1"}]}
# Special: null
