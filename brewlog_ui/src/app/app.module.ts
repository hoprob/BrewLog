import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { MaterialModule } from './material.module';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { SessionComponent } from './session/session.component';
import { RecipeComponent } from './recipe/recipe.component';
import { YeastStarterComponent } from './yeast-starter/yeast-starter.component';
import { MashComponent } from './mash/mash.component';
import { LauterComponent } from './lauter/lauter.component';
import { BoilComponent } from './boil/boil.component';
import { CoolingComponent } from './cooling/cooling.component';
import { FermentationComponent } from './fermentation/fermentation.component';
import { BottlingComponent } from './bottling/bottling.component';
import { LogNoteComponent } from './log-note/log-note.component';

import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import {MatSelectModule} from '@angular/material/select';
import {MatSliderModule} from '@angular/material/slider';
import {MatListModule} from '@angular/material/list';
import { MatInputModule } from '@angular/material/input';
import {MatRadioModule} from '@angular/material/radio';
import { MatFormFieldModule } from "@angular/material/form-field";
import { ApiModule, BASE_PATH } from './bl-api';
import { environment } from 'src/environments/environment';

@NgModule({
  declarations: [
    AppComponent,
    SessionComponent,
    RecipeComponent,
    YeastStarterComponent,
    MashComponent,
    LauterComponent,
    BoilComponent,
    CoolingComponent,
    FermentationComponent,
    BottlingComponent,
    LogNoteComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    MaterialModule,
    ReactiveFormsModule,
    FormsModule,
    ApiModule,
    MatSelectModule,
    HttpClientModule,
    MatSliderModule,
    MatInputModule,
    MatFormFieldModule,
    MatRadioModule,
    MatListModule,
  ],
  providers: [
    {provide: BASE_PATH, useValue: environment.blApiUrl}
  ],
  bootstrap: [AppComponent],
  schemas:[ CUSTOM_ELEMENTS_SCHEMA ]
})
export class AppModule { }
