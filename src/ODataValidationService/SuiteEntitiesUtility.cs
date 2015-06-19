// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace ODataValidator.ValidationService
{
    public static class SuiteEntitiesUtility
    {
        public static ODataValidationSuiteEntities GetODataValidationSuiteEntities()
        {
            // Add for Test project
            if (RuleEngine.DataService.serviceInstance != null)
            {
                RuleEngine.DataService service = new RuleEngine.DataService();
                return new ODataValidationSuiteEntities(service.GetConnectionString());
            }
            else
            {
                return new ODataValidationSuiteEntities();
            }
        }
    }
}