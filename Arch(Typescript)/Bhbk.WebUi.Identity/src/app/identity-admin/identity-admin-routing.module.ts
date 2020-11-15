import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { 
  IdentityAdminActivityComponent,
  IdentityAdminClientComponent,
  IdentityAdminClaimComponent,
  IdentityAdminIssuerComponent,
  IdentityAdminLoginComponent,
  IdentityAdminRoleComponent,
  IdentityAdminUserComponent,
} from 'src/app/identity-admin';
import { AuthorizeGuard } from 'src/app/common/misc';

const routes: Routes = [
  { path: '', canActivate: [AuthorizeGuard], canActivateChild: [AuthorizeGuard],
    children: [
      { path: 'issuer', component: IdentityAdminIssuerComponent },
      { path: 'client', component: IdentityAdminClientComponent },
      { path: 'claim', component: IdentityAdminClaimComponent },
      { path: 'login', component: IdentityAdminLoginComponent },
      { path: 'role', component: IdentityAdminRoleComponent },
      { path: 'user', component: IdentityAdminUserComponent },
      { path: 'activity', component: IdentityAdminActivityComponent },
      { path: '**', pathMatch: "full", redirectTo: '/home' },
    ]},
];

@NgModule({
  imports: [
    RouterModule.forChild(routes),
  ],
  exports: [
    RouterModule,
  ]
})
export class IdentityAdminRoutingModule { }
