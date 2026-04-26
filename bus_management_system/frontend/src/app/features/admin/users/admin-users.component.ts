import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, NgZone } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { AdminUserResponse, roleBadgeTone } from '../../../core/models/admin.models';
import { AdminService } from '../../../core/services/admin.service';
import { AdminBadgeComponent } from '../shared/admin-badge.component';
import { AdminModalComponent } from '../shared/admin-modal.component';
import { AdminTableShellComponent } from '../shared/admin-table-shell.component';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule, AdminModalComponent, AdminTableShellComponent, AdminBadgeComponent],
  templateUrl: './admin-users.component.html',
  styleUrl: './admin-users.component.css',
})
export class AdminUsersComponent {
  isLoading = true;
  isUpdating = false;
  searchTerm = '';
  roleFilter: 'all' | 'user' | 'operator' = 'all';

  message = '';
  messageTone: 'success' | 'danger' = 'success';

  users: AdminUserResponse[] = [];
  filteredUsers: AdminUserResponse[] = [];

  isDeactivateModalOpen = false;
  selectedUser: AdminUserResponse | null = null;

  constructor(
    private readonly adminService: AdminService,
    private readonly zone: NgZone,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.message = '';

    this.adminService
      .getUsers()
      .pipe(
        finalize(() => {
          this.zone.run(() => {
            this.isLoading = false;
            this.cdr.markForCheck();
          });
        })
      )
      .subscribe({
        next: (users) => {
          this.zone.run(() => {
            this.users = users.map((user) => ({
              ...user,
              name: this.safeText(user.name),
              email: this.safeText(user.email),
              phone: this.safeText(user.phone),
              role: this.safeText(user.role),
              isActive: Boolean(user.isActive),
            }));
            this.applyFilters();
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.message = 'Unable to load users right now.';
            this.messageTone = 'danger';
            this.cdr.markForCheck();
          });
        },
      });
  }

  setRoleFilter(filter: 'all' | 'user' | 'operator'): void {
    this.roleFilter = filter;
    this.applyFilters();
  }

  applyFilters(): void {
    const term = this.searchTerm.trim().toLowerCase();
    this.filteredUsers = this.users.filter((user) => {
      const role = this.safeText(user.role).toLowerCase();
      const matchesRole = this.roleFilter === 'all' || role === this.roleFilter;
      const matchesSearch =
        !term || this.safeText(user.name).toLowerCase().includes(term) || this.safeText(user.email).toLowerCase().includes(term);

      return matchesRole && matchesSearch;
    });
  }

  openDeactivateModal(user: AdminUserResponse): void {
    this.selectedUser = user;
    this.isDeactivateModalOpen = true;
  }

  closeDeactivateModal(): void {
    if (this.isUpdating) {
      return;
    }

    this.isDeactivateModalOpen = false;
    this.selectedUser = null;
  }

  deactivateSelectedUser(): void {
    if (!this.selectedUser) {
      return;
    }

    this.isUpdating = true;
    this.message = '';
    const selectedUserId = this.selectedUser.id;

    this.adminService
      .deactivateUser(selectedUserId)
      .pipe(
        finalize(() => {
          this.zone.run(() => {
            this.isUpdating = false;
            this.cdr.markForCheck();
          });
        })
      )
      .subscribe({
        next: () => {
          this.zone.run(() => {
            this.users = this.users.map((user) =>
              user.id === selectedUserId ? { ...user, isActive: false } : user
            );
            this.applyFilters();
            this.closeDeactivateModal();
            this.message = 'User deactivated successfully.';
            this.messageTone = 'success';
            this.cdr.markForCheck();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.message = 'Unable to deactivate user right now.';
            this.messageTone = 'danger';
            this.cdr.markForCheck();
          });
        },
      });
  }

  roleTone(role: string | null | undefined): 'success' | 'warning' | 'danger' | 'info' | 'neutral' {
    return roleBadgeTone(this.safeText(role));
  }

  private safeText(value: unknown): string {
    return typeof value === 'string' ? value.trim() : '';
  }
}
