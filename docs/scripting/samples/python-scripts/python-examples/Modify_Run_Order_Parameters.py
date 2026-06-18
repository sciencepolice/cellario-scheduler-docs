"""
Modify Run Order Parameters

Python scripting example demonstrating Cellario API integration and automation capabilities
"""

#Cellario modules
from HRB.Cellario.Scripting import *
from HRB.Cellario.Scripting.API import *


def Execute(api : PythonScriptingApi):
    if api.CurrentPlate.PlateNumber != 1:
        return
    
    newValue = ""

    pprops = api.CurrentPlate.CurrentProtocol.Properties
    
    if len(pprops) > 0:
        firstProtProp = pprops[0]
        fppPropDescription = firstProtProp.PropertyDescription
        fppPropId = firstProtProp.PropertyId
        fppPropName = firstProtProp.PropertyName
        fppPropValue = firstProtProp.PropertyValue
        
        protPropMsg = "There are " + str(len(pprops)) + " Protocol Properties in this run \n"
        protPropMsg += " The first one: \n"
        protPropMsg += " Name - " + fppPropName + "\n"
        protPropMsg += " Id - " + str(fppPropId) + ";\n"
        protPropMsg += " Value - " + fppPropValue + "; \n"
        protPropMsg += " Description - " + fppPropDescription
        api.Messaging.Notify("Protocol Property", protPropMsg)
    
    else:
        api.Messaging.Notify("Protocol Property", "No Protocol Properties Found")

    pParameters = api.CurrentPlate.CurrentProtocol.Parameters
    
    if len(pParameters) > 0:
        firstProtParam = pParameters[0]
        fppChoicesList = firstProtParam.ChoicesList
        fppDefaultValue = firstProtParam.DefaultValue
        fppDescription = firstProtParam.Description
        fppParameterLevel = firstProtParam.ParameterLevel
        fppParameterName = firstProtParam.ParameterName
        fppParameterType = firstProtParam.ParameterType
        fppProtocolParamaterId = firstProtParam.ProtocolParameterId
        
        protParamMsg = "There are " + str(len(pParameters)) + " Protocol Parameters in this run \n"
        protParamMsg += " The first one: \n"
        protParamMsg += " Name - " + fppParameterName + "\n"
        protParamMsg += " Id - " + str(fppProtocolParamaterId) + "\n"
        protParamMsg += " Type - " + fppParameterType + "\n"
        protParamMsg += " Level - " + fppParameterLevel + "\n"
        protParamMsg += " Value - " + fppDefaultValue + "\n"
        for val in fppChoicesList:
            protParamMsg += " Value Option - " + val + "\n"
            
        protParamMsg += " Description - " + fppDescription
        api.Messaging.Notify("Protocol Parameter", protParamMsg)
        newValue = fppChoicesList[len(fppChoicesList) - 1]
    
    else:
        api.Messaging.Notify("Protocol Parameter", "No Protocol Parameters Found")
        
    roParams = api.CurrentPlate.CurrentRun.RunOrderParameters

    if roParams is not None:
        params = iter(roParams)
        firstRoParams = next(params)
        rParamPName = firstRoParams.ParameterName
        rParamPValue = firstRoParams.ParameterValue
        rParamId = firstRoParams.RunOrderParameterId
        rParampParamId = firstRoParams.ProtocolParameterId

        roParamMsg = "There are " + str(len(pprops)) + " Run Order Parameters in this run \n"
        roParamMsg += " The first one: \n"
        roParamMsg += " Name - " + rParamPName + "\n"
        roParamMsg += " Id - " + str(rParamId) + "\n"
        roParamMsg += " ProtParamId - " + str(rParampParamId) + "\n"
        roParamMsg += " Value - " + rParamPValue + "\n"
        api.Messaging.Notify("Run Order Parameter", roParamMsg)
    else:
        api.Messaging.Notify("Run Order Parameter", "No Run Order Parameters Found")
        
    if (roParams is not None and len(pParameters) > 0):
        params = iter(roParams)
        firstRoParams = next(params)
        oldValue = firstRoParams.ParameterValue
        firstRoParams.ParameterValue = newValue
        modifyMsg = "Changed the Run Order Parameter " + firstRoParams.ParameterName + "\n"
        modifyMsg += " From: " + oldValue + "\n"
        modifyMsg += " To: " + newValue + "\n"
        api.Messaging.Notify("Run Order Parameter Changed", modifyMsg)
    pass 