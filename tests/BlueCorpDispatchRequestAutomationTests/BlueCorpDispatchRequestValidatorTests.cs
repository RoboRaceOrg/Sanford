using BlueCorpDispatchAuto;

public class BlueCorpDispatchRequestValidatorTests
    {
        private string GetValidJson()
        {
            return """
        {
            "controlNumber": 101,
            "salesOrder": "SO123456",
            "containers": [
                {
                    "loadId": "LOAD001",
                    "containerType": "20RF",
                    "items": [
                        {
                            "itemCode": "ITEM001",
                            "quantity": 10,
                            "cartonWeight": 2.5
                        }
                    ]
                }
            ],
            "deliveryAddress": {
                "street": "986 Fake Street",
                "city": "Fake City",
                "state": "Fake State",
                "postalCode": "12345",
                "country": "Fake Country"
            }
        }
        """;
        }


        private string GetNoContainerJson()
        {
            return """
        {
            "controlNumber": 101,
            "salesOrder": "SO123456",
            "deliveryAddress": {
                "street": "986 Fake Street",
                "city": "Fake City",
                "state": "Fake State",
                "postalCode": "12345",
                "country": "Fake Country"
            }
        }
        """;
        }


        private string GetNoDeliveryAddressJson()
        {
            return """
        {
            "controlNumber": 101,
            "salesOrder": "SO123456",
            "containers": [
                {
                    "loadId": "LOAD001",
                    "containerType": "20RF",
                    "items": [
                        {
                            "itemCode": "ITEM001",
                            "quantity": 10,
                            "cartonWeight": 2.5
                        }
                    ]
                }
            ]
        }
        """;
        }

        [Fact]
        public void ValidateJsonData_ShouldReturnEmptyList_WhenJsonIsValid()
        {
            var json = GetValidJson();
            var missingFields = BlueCorpDispatchRequestValidator.ValidateJsonData(json);
            Assert.Empty(missingFields);
        }

        [Fact]
        public void ValidateJsonData_ShouldDetectMissingControlNumber()
        {
            var json = GetValidJson().Replace("\"controlNumber\": 101,", "");
            var missingFields = BlueCorpDispatchRequestValidator.ValidateJsonData(json);
            Assert.Contains("controlNumber", missingFields);
        }

        [Fact]
        public void ValidateJsonData_ShouldDetectMissingSalesOrder()
        {
            var json = GetValidJson().Replace("\"salesOrder\": \"SO123456\",", "");
            var missingFields = BlueCorpDispatchRequestValidator.ValidateJsonData(json);
            Assert.Contains("salesOrder", missingFields);
        }

        [Fact]
        public void ValidateJsonData_ShouldDetectMissingContainers()
        {
            var json = GetNoContainerJson();
            var missingFields = BlueCorpDispatchRequestValidator.ValidateJsonData(json);
            Assert.Contains("containers", missingFields);
        }

        [Fact]
        public void ValidateJsonData_ShouldDetectMissingDeliveryAddress()
        {
            var json = GetNoDeliveryAddressJson();
            var missingFields = BlueCorpDispatchRequestValidator.ValidateJsonData(json);
            Assert.Contains("deliveryAddress", missingFields);
        }

        [Fact]
        public void ValidateJsonData_ShouldDetectMissingDeliveryAddressFields()
        {
            var json = GetValidJson().Replace("\"street\": \"986 Fake Street\",", "");
            var missingFields = BlueCorpDispatchRequestValidator.ValidateJsonData(json);
            Assert.Contains("deliveryAddress.street", missingFields);
        }
    }