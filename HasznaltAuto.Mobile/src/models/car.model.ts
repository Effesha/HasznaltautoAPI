export class Car {
  id: number = 0;

  currentOwner: number = 0;
  currentOwnerName: string | null = '';

  makeId: number = 0;
  makeName: string | null = '';

  modelId: number = 0;
  modelName: string | null = '';

  fuelTypeId: number = 0;
  fuelTypeName: string | null = '';

  price: number = 0;
  mileage: number = 0;
  productionDate: string = '';
  vehicleRegistrationId: number = 0;
  vehicleRegistrationDto: VehicleRegistrationDto = new VehicleRegistrationDto();

  constructor(init?: Partial<Car>) {
    Object.assign(this, init);
  }
}

export class VehicleRegistrationDto {
  licensePlate: string | null = '';
}
