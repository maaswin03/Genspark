import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { catchError, map, Observable, of, shareReplay } from 'rxjs';

export interface LocationOption {
  id: number;
  name: string;
  city: string;
  state: string;
}

type LocationApiResponse = LocationOption;

@Injectable({
  providedIn: 'root',
})
export class LocationService {
  private locationsCache$?: Observable<LocationOption[]>;
  private readonly apiBaseUrl = 'http://localhost:5131';

  constructor(
    private readonly http: HttpClient,
    @Inject(PLATFORM_ID) private readonly platformId: object
  ) {}

  getLocations(): Observable<LocationOption[]> {
    if (!isPlatformBrowser(this.platformId)) {
      return new Observable<LocationOption[]>((subscriber) => {
        subscriber.next([]);
        subscriber.complete();
      });
    }

    if (!this.locationsCache$) {
      this.locationsCache$ = this.fetchLocations().pipe(shareReplay(1));
    }

    return this.locationsCache$;
  }

  private fetchLocations(): Observable<LocationOption[]> {
    return this.http.get<LocationApiResponse[]>(`${this.apiBaseUrl}/api/locations`).pipe(
      map((locations) => locations.map((location) => ({
        id: location.id,
        name: location.name,
        city: location.city,
        state: location.state,
      }))),
      catchError(() => of([]))
    );
  }
}
