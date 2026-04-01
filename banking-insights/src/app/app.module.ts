import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppComponent } from './app.component';
import { RouterModule } from '@angular/router';
import { CoreModule } from './core/core.module';
import { LoginModule } from './pages/login/login.module';
import { SharedModule } from './shared/shared.module';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppRoutingModule } from './app.routing.module';
import { CommonModule, HashLocationStrategy, LocationStrategy } from '@angular/common';
import { ConfigService } from './services/config.service';
import { HttpClientModule, provideHttpClient, withInterceptors } from '@angular/common/http';
import { StoreModule } from '@ngrx/store';
import { reducers } from './store/root-reducer';
import { localStorageSyncReducer, rehydrateState } from './store/persist-meta.reduser';
import { jwtInterceptor } from './core/interceptors/JWT/jwt.interceptor';
import { ToasterContainerComponent } from './shared/components/toaster-container/toaster-container.component';

// Import other components, directives, pipes here

export function initializeApp(appConfig: ConfigService) {
  return () => appConfig.loadConfig();
}

@NgModule({
  declarations: [
    AppComponent,
    // ToasterContainerComponent
  ],
  imports: [
    BrowserModule,
    CommonModule,
    // AppRoutingModule,
    RouterModule,
    SharedModule,
    CoreModule,
    BrowserAnimationsModule,
    AppRoutingModule,

    HttpClientModule,
    LoginModule,    // List other components here
    // RouterModule.forRoot(routes) // 3. Initialize routing here

    // Add other necessary modules like routing, forms, http here
    StoreModule.forRoot(reducers, {
      metaReducers: [localStorageSyncReducer],
      initialState: rehydrateState(),
    }),
  ],
  providers: [
    { provide: LocationStrategy, useClass: HashLocationStrategy },
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApp,
      deps: [ConfigService],
      multi: true,
    },
       provideHttpClient(
      withInterceptors([jwtInterceptor])
    )
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
