import { Routes } from '@angular/router';
import { RenderMode, ServerRoute } from '@angular/ssr';
import { TripSearchComponent } from './features/user/trip-search/trip-search.component';
import { TripsComponent } from './features/user/trips/trips.component';
import { SeatSelectionComponent } from './features/user/seat-selection/seat-selection.component';
import { PassengerDetailsComponent } from './features/user/passenger-details/passenger-details.component';
import { PaymentComponent } from './features/user/payment/payment.component';
import { BookingConfirmationComponent } from './features/user/booking-confirmation/booking-confirmation.component';
import { BookingListComponent } from './features/user/bookings/booking-list/booking-list.component';
import { BookingDetailComponent } from './features/user/bookings/booking-detail/booking-detail.component';
import { NotificationsComponent } from './features/user/notifications/notifications.component';
import { ProfileComponent } from './features/user/profile/profile.component';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { OperatorRegisterComponent } from './features/auth/operator-register/operator-register.component';
import { UnauthorizedComponent } from './features/auth/unauthorized/unauthorized.component';
import { NotFoundComponent } from './features/auth/not-found/not-found.component';
import { AdminLayoutComponent } from './features/admin/admin-layout/admin-layout.component';
import { AdminDashboardComponent } from './features/admin/dashboard/admin-dashboard.component';
import { AdminOperatorsComponent } from './features/admin/operators/admin-operators.component';
import { OperatorDetailComponent } from './features/admin/operators/operator-detail.component';
import { AdminLocationsComponent } from './features/admin/locations/admin-locations.component';
import { AdminRoutesComponent } from './features/admin/routes/admin-routes.component';
import { AdminUsersComponent } from './features/admin/users/admin-users.component';
import { AdminRevenueComponent } from './features/admin/revenue/admin-revenue.component';
import { AdminPlatformFeeComponent } from './features/admin/platform-fee/admin-platform-fee.component';
import { OperatorLayoutComponent } from './features/operator/operator-layout/operator-layout.component';
import { OperatorDashboardComponent } from './features/operator/dashboard/operator-dashboard.component';
import { OperatorBusesComponent } from './features/operator/buses/operator-buses.component';
import { OperatorBookingsComponent } from './features/operator/bookings/operator-bookings.component';
import { OperatorTripsComponent } from './features/operator/trips/operator-trips.component';
import { OperatorTripDetailComponent } from './features/operator/trips/operator-trip-detail.component';
import { OperatorRevenueComponent } from './features/operator/revenue/operator-revenue.component';
import { OperatorOfficesComponent } from './features/operator/offices/operator-offices.component';
import { OperatorProfileComponent } from './features/operator/profile/operator-profile.component';
import { OperatorStubPageComponent } from './features/operator/stub-page/operator-stub-page.component';
import { adminGuard } from './core/guards/admin.guard';
import { operatorGuard } from './core/guards/operator.guard';
import { userGuard } from './core/guards/user.guard';
import { guestGuard } from './core/guards/guest.guard';

import { UserLayout } from './features/user/user-layout/user-layout';

export const routes: Routes = [
  {
    path: '',
    component: UserLayout,
    children: [
      {
        path: '',
        redirectTo: 'home',
        pathMatch: 'full',
      },
      {
        path: 'home',
        component: TripSearchComponent,
      },
      {
        path: 'trips/search',
        component: TripsComponent,
      },
      {
        path: 'trips/:id/seats',
        component: SeatSelectionComponent,
        canActivate: [userGuard],
      },
      {
        path: 'bookings/passengers',
        component: PassengerDetailsComponent,
        canActivate: [userGuard],
      },
      {
        path: 'bookings/:id/payment',
        component: PaymentComponent,
        canActivate: [userGuard],
      },
      {
        path: 'bookings/:id/confirmation',
        component: BookingConfirmationComponent,
        canActivate: [userGuard],
      },
      {
        path: 'user/bookings',
        component: BookingListComponent,
        canActivate: [userGuard],
      },
      {
        path: 'user/bookings/:id',
        component: BookingDetailComponent,
        canActivate: [userGuard],
      },
      {
        path: 'user/notifications',
        component: NotificationsComponent,
        canActivate: [userGuard],
      },
      {
        path: 'user/profile',
        component: ProfileComponent,
        canActivate: [userGuard],
      },
      {
        path: 'trips',
        redirectTo: 'trips/search',
        pathMatch: 'full',
      },
      {
        path: 'user',
        canActivate: [userGuard],
        children: []
      }
    ]
  },
  {
    path: 'admin',
    component: AdminLayoutComponent,
    canActivate: [adminGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        component: AdminDashboardComponent,
      },
      {
        path: 'operators',
        component: AdminOperatorsComponent,
      },
      {
        path: 'operators/:id',
        component: OperatorDetailComponent,
      },
      {
        path: 'locations',
        component: AdminLocationsComponent,
      },
      {
        path: 'routes',
        component: AdminRoutesComponent,
      },
      {
        path: 'users',
        component: AdminUsersComponent,
      },
      {
        path: 'revenue',
        component: AdminRevenueComponent,
      },
      {
        path: 'platform-fee',
        component: AdminPlatformFeeComponent,
      },
    ],
  },
  {
    path: 'operator',
    component: OperatorLayoutComponent,
    canActivate: [operatorGuard],
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        component: OperatorDashboardComponent,
      },
      {
        path: 'buses',
        component: OperatorBusesComponent,
      },
      {
        path: 'trips',
        component: OperatorTripsComponent,
      },
      {
        path: 'trips-schedules',
        redirectTo: 'trips',
        pathMatch: 'full',
      },
      {
        path: 'trips/:id',
        component: OperatorTripDetailComponent,
      },
      {
        path: 'bookings',
        component: OperatorBookingsComponent,
      },
      {
        path: 'bookings-schedule',
        redirectTo: 'trips',
        pathMatch: 'full',
      },
      {
        path: 'revenue',
        component: OperatorRevenueComponent,
      },
      {
        path: 'offices',
        component: OperatorOfficesComponent,
      },
      {
        path: 'profile',
        component: OperatorProfileComponent,
      },
      {
        path: 'notifications',
        component: OperatorStubPageComponent,
        data: { title: 'Notifications' },
      },
    ],
  },
  {
    path: 'auth/login',
    component: LoginComponent,
    canActivate: [guestGuard],
  },
  {
    path: 'auth/register',
    component: RegisterComponent,
    canActivate: [guestGuard],
  },
  {
    path: 'auth/operator-register',
    component: OperatorRegisterComponent,
    canActivate: [guestGuard],
  },
  {
    path: 'unauthorized',
    component: UnauthorizedComponent,
  },
  {
    path: 'not-found',
    component: NotFoundComponent,
  },
  {
    path: '**',
    redirectTo: 'not-found',
  },
];

export const serverRoutes: ServerRoute[] = [
  {
    path: '**',
    renderMode: RenderMode.Client,
  },
];
