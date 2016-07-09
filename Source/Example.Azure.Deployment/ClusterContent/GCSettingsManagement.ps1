<#
//*********************************************************
//
//    Copyright (c) Microsoft. All rights reserved.
//    This code is licensed under the Microsoft Public License.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
#>
Param
(
	$serverGC = $True,
	$backgroundGC = $False
)

[string]$configFilePath = "$(${env:RoleRoot})\base\x64\WaWorkerHost.exe.config"

function Create-ConfigFileIfNotExists
{
	# Only create the Xml document if it does not already exist
	if(-not (Test-Path -Path $configFilePath -PathType Leaf))
	{
		[System.Xml.XmlDocument]$document = New-Object System.Xml.XmlDocument
		
		# config file doesn't exist create a now one
 		[System.Xml.XmlDeclaration]$prolog = $document.CreateXmlDeclaration("1.0", "utf-8", $null)
 		[System.Xml.XmlNode]$child = $document.AppendChild($prolog)
		[System.Xml.XmlElement]$configurationElement = Append-ElementIfNotExists $document $document.DocumentElement "configuration"
		
		# Save a copy of the document
		$document.Save($configFilePath)
	}
}

function Load-ConfigFile
{
	[System.Xml.XmlDocument]$document = New-Object System.Xml.XmlDocument
	
	#Check if the document already exists and load it if it does not
	if(Test-Path -Path $configFilePath -PathType Leaf)
	{
		$document.Load($configFilePath)
	}
	
	return $document
}

function Append-ElementIfNotExists
{
	param
	(
		[System.Xml.XmlDocument]$document,
		[System.Xml.XmlElement]$parent,
		[string]$elementName
	)
	[System.Xml.XmlElement]$element = $null
	[System.Xml.XmlNode]$parentNode = $parent
	
	if($document -ne $null)
	{
		if($parentNode -eq $null)
		{
			$parentNode = $document
		}
		
		$element = $parentNode.SelectSingleNode("./$($elementName)")
		
		if($element -eq $null)
		{
			$element = $document.CreateElement($elementName)
			[System.Xml.XmlElement]$child = $parentNode.AppendChild($element)
		}
	}
	
	return $element
}

function Create-ElementStructureIfNotExists
{
	param
	(
		[System.Xml.XmlDocument]$document
	)
	[bool]$isSuccess = $false
	
	if($document -ne $null)
	{
		[System.Xml.XmlElement]$configurationElement = Append-ElementIfNotExists $document $null "configuration"
		
		if($configurationElement -ne $null)
		{
			[System.Xml.XmlElement]$element = Append-ElementIfNotExists $document $configurationElement "runtime"
			
			$isSuccess = $element -ne $null
		}
	}
	
	return $isSuccess
}

# Create the document if required
Create-ConfigFileIfNotExists

# Load the configuration file into the XML document
[System.Xml.XmlDocument]$configurationDocument = Load-ConfigFile
	
if($configurationDocument -ne $null)
{
	if(Create-ElementStructureIfNotExists $configurationDocument)
	{
		# All of the entries are on the runtime element
		[System.Xml.XmlElement]$runtimeElement = $configurationDocument.DocumentElement.SelectSingleNode('./runtime')
		
		if($runtimeElement -ne $null)
		{
			# Set the Server GC to enabled if requested
			[System.Xml.XmlElement]$serverGCElement = Append-ElementIfNotExists $configurationDocument $runtimeElement "gcServer"
			$serverGCElement.SetAttribute("enabled", $serverGC.ToString([System.Globalization.CultureInfo]::InvariantCulture).ToLower()) 

			# Set the concurrent GC to enabled if requested
			[System.Xml.XmlElement]$concurrentGCElement = Append-ElementIfNotExists $configurationDocument $runtimeElement "gcConcurrent"
			$concurrentGCElement.SetAttribute("enabled", $backgroundGC.ToString([System.Globalization.CultureInfo]::InvariantCulture).ToLower()) 
		}
	}
	
	# Save the document
	$configurationDocument.Save($configFilePath)
}



 
 
