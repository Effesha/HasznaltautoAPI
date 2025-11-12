import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Car } from '../models/car.model'
import { environment } from '../environments/environment'

@Injectable({ providedIn: 'root' })
export class HasznaltAutoService {
  private baseUrl = `${environment.apiBaseUrl}/hasznaltauto`;

  constructor(private http: HttpClient) { }

  list(): Observable<Car[]> {
    return this.http.get<Car[]>(this.baseUrl);
  }

  get(id: number): Observable<Car> {
    return this.http.get<Car>(`${this.baseUrl}/${id}`);
  }

  create(item: Car): Observable<Car> {
    return this.http.post<Car>(this.baseUrl, item);
  }

  update(id: number, item: Car): Observable<Car> {
    return this.http.put<Car>(`${this.baseUrl}/${id}`, item);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
