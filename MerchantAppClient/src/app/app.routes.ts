import { Routes } from '@angular/router';
import { Layouts } from './components/layouts/layouts';
import { Home } from './components/home/home';
import { Login } from './components/login/login';
import { NotFound } from './components/not-found/not-found';

export const routes: Routes = [
  {
    path: '',
    component: Layouts,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'basvuru' },
      { path: 'basvuru', component: Home },
      { path: 'izleme', component: Login }
    ]
  },
  { path: '**', component: NotFound }
];
