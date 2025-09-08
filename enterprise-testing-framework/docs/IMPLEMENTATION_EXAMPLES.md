# Implementation Examples
## Enterprise YAML-Based Testing Framework

This document showcases the technical sophistication and practical applications of the testing framework through comprehensive examples that demonstrate enterprise-grade capabilities.

---

## 🚀 Advanced Multi-Protocol Integration Testing

### Example 1: End-to-End Microservices Testing

```yaml
Name: "E2E Microservices Integration Test"
Description: "Demonstrates complex workflow testing across multiple protocols and services"

Variables:
  apiBaseUrl: "https://api.enterprise.com"
  authToken: ""
  userId: ""
  orderData: ""

Steps:
  # Step 1: Authentication via HTTP
  - Name: "Authenticate User"
    Type: "http"
    Input:
      Method: "POST"
      RequestUri: "{{$var:apiBaseUrl}}/auth/login"
      Headers:
        Content-Type: "application/json"
        X-API-Version: "v2"
      Body:
        username: "{{$var:testUser}}"
        password: "{{$var:testPassword}}"
        clientId: "test-automation"
    
    Output:
      Format: "json"
      Store:
        authToken: "output.content.json.accessToken"
        userId: "output.content.json.user.id"
    
    Asserters:
      - AssertEq:
          JPathExpr: "$curStep:output.statusCode"
          ConstExpr: 200
          ErrorMessage: "Authentication should succeed"
      
      - AssertNotNull:
          JPathExpr: "$var:authToken"
          ErrorMessage: "Auth token must be present"
      
      - AssertLt:
          JPathExpr: "$curStep:performance.executionTimeMs"
          ConstExpr: 2000
          ErrorMessage: "Authentication should complete within 2 seconds"

  # Step 2: Create Order via RPC
  - Name: "Create Order via gRPC"
    Type: "rpc"
    Input:
      ProtoTypeName: "OrderService.CreateOrderReq"
      ServerIp: "{{$var:grpcServerIp}}"
      ServerPort: "{{$var:grpcServerPort}}"
      Headers:
        authorization: "Bearer {{$var:authToken}}"
      Body:
        userId: "{{$var:userId}}"
        items:
          - productId: "PROD-001"
            quantity: 2
            price: 29.99
          - productId: "PROD-002"
            quantity: 1
            price: 49.99
        shippingAddress:
          street: "123 Test Street"
          city: "Test City"
          zipCode: "12345"
    
    Output:
      Format: "protobuf"
      Store:
        orderId: "output.content.orderId"
        orderTotal: "output.content.totalAmount"
    
    Asserters:
      - AssertEq:
          JPathExpr: "$curStep:output.Error.Code"
          ConstExpr: 0
          ErrorMessage: "Order creation should succeed"
      
      - AssertNotNull:
          JPathExpr: "$var:orderId"
          ErrorMessage: "Order ID must be generated"
      
      - AssertEq:
          JPathExpr: "$var:orderTotal"
          ConstExpr: 109.97
          ErrorMessage: "Order total should be calculated correctly"

  # Step 3: Validate Order via Message Bus
  - Name: "Validate Order Processing"
    Type: "mbus"
    Input:
      ServerIp: "{{$var:mbusServerIp}}"
      ServerPort: "{{$var:mbusServerPort}}"
      MessageType: "OrderValidationReq"
      Body:
        orderId: "{{$var:orderId}}"
        validationType: "FULL_VALIDATION"
    
    Execution:
      MaxRetries: 5
      RetryDelayMs: 1000
      RetryUntil:
        - AssertEq:
            JPathExpr: "$curStep:output.content.status"
            ConstExpr: "VALIDATED"
    
    Output:
      Store:
        validationStatus: "output.content.status"
        validationDetails: "output.content.details"
    
    Asserters:
      - AssertEq:
          JPathExpr: "$var:validationStatus"
          ConstExpr: "VALIDATED"
          ErrorMessage: "Order should be validated successfully"

  # Step 4: Generate Report via Script
  - Name: "Generate Order Report"
    Type: "script"
    Input:
      Language: "python"
      Parameters:
        orderId: "{{$var:orderId}}"
        orderTotal: "{{$var:orderTotal}}"
        validationDetails: "{{$var:validationDetails}}"
      Script: |
        import json
        import os
        from datetime import datetime
        
        # Extract parameters
        order_id = os.environ.get('orderId')
        order_total = float(os.environ.get('orderTotal', '0'))
        validation_details = json.loads(os.environ.get('validationDetails', '{}'))
        
        # Generate comprehensive report
        report = {
            "reportId": f"RPT-{order_id}-{datetime.now().strftime('%Y%m%d%H%M%S')}",
            "orderId": order_id,
            "orderTotal": order_total,
            "validationStatus": validation_details.get('status'),
            "processingTime": validation_details.get('processingTimeMs', 0),
            "reportGeneratedAt": datetime.now().isoformat(),
            "summary": {
                "totalItems": len(validation_details.get('items', [])),
                "validationPassed": validation_details.get('status') == 'VALIDATED',
                "riskScore": validation_details.get('riskScore', 0)
            }
        }
        
        print("RESULT_JSON:" + json.dumps(report, indent=2))
    
    Output:
      Format: "text"
      Store:
        reportData: "output.output"
    
    Asserters:
      - AssertEq:
          JPathExpr: "$curStep:output.exitCode"
          ConstExpr: 0
          ErrorMessage: "Report generation should succeed"
```

---

## 🎯 Performance Testing with Advanced Analytics

### Example 2: Load Testing with Performance Validation

```yaml
Name: "API Performance Load Test"
Description: "Comprehensive performance testing with threshold validation and analytics"

Tables:
  - Name: "LoadTestScenarios"
    Data:
      - scenario: "light_load"
        concurrent_users: 10
        duration_seconds: 60
        expected_avg_response_ms: 200
        expected_95th_percentile_ms: 500
      - scenario: "medium_load"
        concurrent_users: 50
        duration_seconds: 120
        expected_avg_response_ms: 400
        expected_95th_percentile_ms: 800
      - scenario: "heavy_load"
        concurrent_users: 100
        duration_seconds: 180
        expected_avg_response_ms: 600
        expected_95th_percentile_ms: 1200

RepeatFor:
  Table: "LoadTestScenarios"

Steps:
  - Name: "Performance Test - {{$table:scenario}}"
    Type: "script"
    Input:
      Language: "python"
      Parameters:
        apiEndpoint: "{{$var:apiBaseUrl}}/api/orders"
        concurrentUsers: "{{$table:concurrent_users}}"
        durationSeconds: "{{$table:duration_seconds}}"
        authToken: "{{$var:authToken}}"
      Script: |
        import asyncio
        import aiohttp
        import time
        import statistics
        import json
        import os
        
        async def make_request(session, url, headers):
            start_time = time.time()
            try:
                async with session.get(url, headers=headers) as response:
                    await response.text()
                    return {
                        'status_code': response.status,
                        'response_time_ms': (time.time() - start_time) * 1000,
                        'success': response.status == 200
                    }
            except Exception as e:
                return {
                    'status_code': 0,
                    'response_time_ms': (time.time() - start_time) * 1000,
                    'success': False,
                    'error': str(e)
                }
        
        async def load_test():
            endpoint = os.environ.get('apiEndpoint')
            concurrent_users = int(os.environ.get('concurrentUsers', '10'))
            duration = int(os.environ.get('durationSeconds', '60'))
            auth_token = os.environ.get('authToken')
            
            headers = {'Authorization': f'Bearer {auth_token}'}
            
            results = []
            start_time = time.time()
            
            async with aiohttp.ClientSession() as session:
                while time.time() - start_time < duration:
                    tasks = [
                        make_request(session, endpoint, headers)
                        for _ in range(concurrent_users)
                    ]
                    batch_results = await asyncio.gather(*tasks)
                    results.extend(batch_results)
                    
                    # Brief pause between batches
                    await asyncio.sleep(0.1)
            
            # Calculate performance metrics
            response_times = [r['response_time_ms'] for r in results if r['success']]
            success_count = sum(1 for r in results if r['success'])
            
            if response_times:
                metrics = {
                    'total_requests': len(results),
                    'successful_requests': success_count,
                    'failed_requests': len(results) - success_count,
                    'success_rate': success_count / len(results) * 100,
                    'avg_response_time_ms': statistics.mean(response_times),
                    'median_response_time_ms': statistics.median(response_times),
                    'min_response_time_ms': min(response_times),
                    'max_response_time_ms': max(response_times),
                    'p95_response_time_ms': statistics.quantiles(response_times, n=20)[18],
                    'p99_response_time_ms': statistics.quantiles(response_times, n=100)[98],
                    'requests_per_second': len(results) / duration,
                    'concurrent_users': concurrent_users,
                    'test_duration_seconds': duration
                }
            else:
                metrics = {'error': 'No successful requests'}
            
            print("RESULT_JSON:" + json.dumps(metrics, indent=2))
        
        asyncio.run(load_test())
    
    Output:
      Format: "text"
      Store:
        performanceMetrics: "output.output"
    
    Asserters:
      # Validate test execution
      - AssertEq:
          JPathExpr: "$curStep:output.exitCode"
          ConstExpr: 0
          ErrorMessage: "Load test should execute successfully"
      
      # Performance threshold validations
      - AssertGte:
          JPathExpr: "$curStep:output.parsed.success_rate"
          ConstExpr: 95.0
          ErrorMessage: "Success rate should be at least 95%"
      
      - AssertLte:
          JPathExpr: "$curStep:output.parsed.avg_response_time_ms"
          ConstExpr: "{{$table:expected_avg_response_ms}}"
          ErrorMessage: "Average response time should meet expectations"
      
      - AssertLte:
          JPathExpr: "$curStep:output.parsed.p95_response_time_ms"
          ConstExpr: "{{$table:expected_95th_percentile_ms}}"
          ErrorMessage: "95th percentile should meet expectations"
      
      # Framework performance validation
      - AssertLt:
          JPathExpr: "$curStep:performance.executionTimeMs"
          ConstExpr: 300000  # 5 minutes max
          ErrorMessage: "Load test should complete within time limit"
```

---

## 🔧 Advanced Data Processing and Validation

### Example 3: Complex Data Transformation Pipeline

```yaml
Name: "Data Processing Pipeline Test"
Description: "Demonstrates complex data transformation and validation capabilities"

Steps:
  - Name: "Extract Data from Multiple Sources"
    Type: "script"
    Input:
      Language: "python"
      Parameters:
        dataSource1: "{{$var:csvDataPath}}"
        dataSource2: "{{$var:jsonDataPath}}"
        dataSource3: "{{$var:xmlDataPath}}"
      Script: |
        import pandas as pd
        import json
        import xml.etree.ElementTree as ET
        import os
        
        # Load data from multiple sources
        csv_path = os.environ.get('dataSource1')
        json_path = os.environ.get('dataSource2')
        xml_path = os.environ.get('dataSource3')
        
        # Process CSV data
        csv_data = pd.read_csv(csv_path) if csv_path else pd.DataFrame()
        
        # Process JSON data
        with open(json_path, 'r') as f:
            json_data = json.load(f) if json_path else {}
        
        # Process XML data
        xml_data = []
        if xml_path:
            tree = ET.parse(xml_path)
            root = tree.getroot()
            for item in root.findall('.//record'):
                xml_data.append({
                    'id': item.get('id'),
                    'value': item.text
                })
        
        # Combine and transform data
        combined_data = {
            'csv_records': len(csv_data),
            'json_records': len(json_data.get('records', [])),
            'xml_records': len(xml_data),
            'total_records': len(csv_data) + len(json_data.get('records', [])) + len(xml_data),
            'data_sources': ['csv', 'json', 'xml'],
            'processing_timestamp': pd.Timestamp.now().isoformat()
        }
        
        print("RESULT_JSON:" + json.dumps(combined_data, indent=2))
    
    Output:
      Format: "text"
      Store:
        extractedData: "output.output"
    
    Asserters:
      - AssertGt:
          JPathExpr: "$curStep:output.parsed.total_records"
          ConstExpr: 0
          ErrorMessage: "Should extract data from sources"
```

*This document demonstrates the framework's capability to handle complex, real-world enterprise testing scenarios with sophisticated data processing, multi-protocol integration, and advanced performance analytics.*
