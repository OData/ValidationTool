mkdir ..\bin\rulestore
copy ..\..\XMLRules\Common\* ..\bin\rulestore\*
copy ..\..\XMLRules\\ServiceDocument\* ..\bin\rulestore\*
copy ..\..\XMLRules\\MetadataDocument\* ..\bin\rulestore\*
copy ..\..\XMLRules\\Feed\* ..\bin\rulestore\*
copy ..\..\XMLRules\\Entry\* ..\bin\rulestore\*
copy ..\..\XMLRules\\Error\* ..\bin\rulestore\*
copy ..\..\XMLRules\\IndividualProperty\* ..\bin\rulestore\*
copy ..\..\XMLRules\\EntityReference\* ..\bin\rulestore\*

mkdir ..\bin\extensions
copy ..\..\CodeRules\bin\ODataValidator.CodeRules.dll ..\bin\extensions\ODataValidator.CodeRules.dll
copy ..\..\CodeRules\bin\ODataValidator.CodeRules.pdb ..\bin\extensions\ODataValidator.CodeRules.pdb