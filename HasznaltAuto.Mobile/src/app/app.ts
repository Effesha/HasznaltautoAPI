import { Component, OnInit } from '@angular/core';
import { HasznaltAutoService } from '../services/hasznalt-auto.service';
import { Car } from '../models/car.model';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false
})
export class App implements OnInit {
  cars: Car[] = [];
  selected: Car | null = null;
  editModel: Car = new Car();
  isNew = false;
  error: string | null = null;

  constructor(private service: HasznaltAutoService) { }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.service.list().subscribe({
      next: data => { this.cars = data; this.error = null; },
      error: err => { this.error = 'Hiba a lista betöltésénél'; console.error(err); }
    });
    this.cancel();
  }

  get(item: Car): void {
    this.service.get(item.id!).subscribe({
      next: data => { this.selected = data; this.error = null; },
      error: err => { this.error = 'Hiba a részletek betöltésénél'; console.error(err); }
    });
  }

  newCar(): void {
    this.isNew = true;
    this.editModel = new Car();
    this.selected = null;
  }

  edit(item: Car): void {
    this.isNew = false;
    this.editModel = { ...item };
    this.selected = null;
  }

  save(): void {
    if (this.isNew) {
      this.service.create(this.editModel).subscribe({
        next: () => this.load(),
        error: err => { this.error = 'Hiba létrehozáskor'; console.error(err); }
      });
    } else {
      if (!this.editModel.id) { this.error = 'Nincs id a frissítéshez'; return; }
      this.service.update(this.editModel.id, this.editModel).subscribe({
        next: () => this.load(),
        error: err => { this.error = 'Hiba frissítéskor'; console.error(err); }
      });
    }
  }

  remove(item: Car): void {
    if (!item.id) return;
    this.service.delete(item.id).subscribe({
      next: () => this.load(),
      error: err => { this.error = 'Hiba törléskor'; console.error(err); }
    });
  }

  cancel(): void {
    this.isNew = false;
    this.selected = null;
    this.editModel = new Car();
    this.error = null;
  }
}
