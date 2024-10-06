## XML to CSV Converter

This .NET Core 8 console application converts a directory of XML files with identical structure into a single CSV file. The application dynamically determines the CSV columns based on the first XML file's structure.

### Features

* **Dynamic column generation:**  The CSV header and columns are created automatically by analyzing the structure of the first XML file in the input directory.
* **Handles nested lists:** The application can process XML files containing nested lists, creating a separate CSV row for each item in the deeply nested lists.
* **Tag-based analysis:**  The converter analyzes XML tags to identify lists and nested objects for accurate data extraction.
* **Error handling:** Basic error handling is implemented to ensure smooth execution and informative error messages.


### Input/Output Example

**Input XML (example):**

```xml
<?xml version="1.0"?>
<Root>
	<Title>secondDoc</Title>
	<customers>
		<groupNumber>4</groupNumber>
		<groupId>4</groupId>
		<customer>
			<name>Charter Group</name>
			<address>
				<addres>
					<street>100 Main</street>
					<city>Framingham</city>
					<state>MA</state>
					<zip>01701</zip>
				</addres>
				<addres>
					<street>720 Prospect</street>
					<state>MA</state>
					<zip>01701</zip>
				</addres>
				<addres>
					<street>120 Ridge</street>
					<state>MA</state>
					<zip>01760</zip>
					<city>Framingham</city>
				</addres>
			</address>
			<id>
				<value>1</value>
			</id>
		</customer>
		<customer>
			<name>Charter Group2</name>
			<address>
				<addres>
					<street>100 Main2</street>
					<city>Framingham2</city>
					<state>MA2</state>
					<zip>017012</zip>
				</addres>
				<addres>
					<street>720 Prospect2</street>
					<city>Framingham</city>
					<state>MA</state>
					<zip>01701</zip>
				</addres>
				<addres>
					<street>120 Ridge2</street>
					<state>MA</state>
					<zip>017602</zip>
					<city/>
				</addres>
			</address>
			<id>
				<value>2</value>
			</id>
		</customer>
	</customers>
</Root>
```

**Output CSV:**

```
Title,groupNumber,groupId,name,street,city,state,zip,value
secondDoc,4,4,Charter Group,100 Main,Framingham,MA,01701,1
secondDoc,4,4,Charter Group,720 Prospect,,MA,01701,1
secondDoc,4,4,Charter Group,120 Ridge,Framingham,MA,01760,1
secondDoc,4,4,Charter Group2,100 Main2,Framingham2,MA2,017012,2
secondDoc,4,4,Charter Group2,720 Prospect2,Framingham,MA,01701,2
secondDoc,4,4,Charter Group2,120 Ridge2,,MA,017602,2
```

### Notes

* All XML files in the input directory must have the same structure.
* The first XML file in the directory is used to determine the CSV structure.
* The application assumes that all elements within a list have the same structure.


### Future Enhancements

* Implement more robust error handling and validation for different XML structures.
* Add support for specifying custom delimiters and text qualifiers for the output CSV file.
* Provide an option to specify the XML node to start extracting data from, allowing for greater flexibility.
* Create a user interface for easier interaction with the application. 
