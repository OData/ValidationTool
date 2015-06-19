mkdir ..\bin\bin
copy ..\bin\ODataValidator.ValidationService.dll ..\bin\bin\ODataValidator.ValidationService.dll
copy ..\bin\ODataValidator.ValidationService.pdb ..\bin\bin\ODataValidator.ValidationService.pdb
copy ..\bin\ODataValidator.RuleEngine.dll ..\bin\bin\ODataValidator.RuleEngine.dll
copy ..\bin\ODataValidator.RuleEngine.pdb ..\bin\bin\ODataValidator.RuleEngine.pdb
copy ..\bin\Newtonsoft.Json.dll ..\bin\bin\Newtonsoft.Json.dll
copy ..\bin\Commons.Xml.Relaxng.dll ..\bin\bin\Commons.Xml.Relaxng.dll

mkdir ..\bin\bin\rulestore
copy ..\..\XMLRules\Common\* ..\bin\bin\rulestore\*
copy ..\..\XMLRules\\ServiceDocument\* ..\bin\bin\rulestore\*
copy ..\..\XMLRules\\MetadataDocument\* ..\bin\bin\rulestore\*
copy ..\..\XMLRules\\Feed\* ..\bin\bin\rulestore\*
copy ..\..\XMLRules\\Entry\* ..\bin\bin\rulestore\*
copy ..\..\XMLRules\\Error\* ..\bin\bin\rulestore\*
copy ..\..\XMLRules\\IndividualProperty\* ..\bin\bin\rulestore\*
copy ..\..\XMLRules\\EntityReference\* ..\bin\bin\rulestore\*

mkdir ..\bin\bin\extensions
copy ..\..\CodeRules\bin\ODataValidator.CodeRules.dll ..\bin\bin\extensions\ODataValidator.CodeRules.dll
copy ..\..\CodeRules\bin\ODataValidator.CodeRules.pdb ..\bin\bin\extensions\ODataValidator.CodeRules.pdb