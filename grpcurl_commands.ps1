$endpoints = @(
    @{ name = 'UserGrpc.Login'; data = '{\"user\":{\"name\":\"first_user\",\"password\":\"asd123\"}}'; },
    @{ name = 'HasznaltAutoGrpc.GetCar'; data = '{\"car_id\":1}'; },
    @{ name = 'HasznaltAutoGrpc.ListCars'; }
    @{ name = 'HasznaltAutoGrpc.ListCarsFiltered'; data = '{\"model\":\"Astra\"}';}
)

foreach ($endpoint in $endpoints) {
    Write-Host "Calling: $($endpoint.name)"

    if ($endpoint.ContainsKey("data")){
        grpcurl -plaintext -d $endpoint.data localhost:1337 $endpoint.name
    } else {
        grpcurl -plaintext localhost:1337 $endpoint.name
    }

    Write-Host ""
}