import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SessionComponent } from './session/session.component';
import { RecipeComponent } from './recipe/recipe.component';
import { YeastStarterComponent } from './yeast-starter/yeast-starter.component';
import { MashComponent } from './mash/mash.component';
import { LauterComponent } from './lauter/lauter.component';
import { BoilComponent } from './boil/boil.component';
import { CoolingComponent } from './cooling/cooling.component';
import { FermentationComponent } from './fermentation/fermentation.component';
import { BottlingComponent } from './bottling/bottling.component';

const routes: Routes = [
  {path: 'session', component: SessionComponent},
  {path: '', component: SessionComponent},
  {path: 'recipe', component: RecipeComponent},
  {path: 'yeast-starter', component: YeastStarterComponent},
  {path: 'mash', component: MashComponent},
  {path: 'lauter', component: LauterComponent},
  {path: 'boil', component: BoilComponent},
  {path: 'cooling', component: CoolingComponent},
  {path: 'fermentation', component: FermentationComponent},
  {path: 'bottling', component: BottlingComponent},
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
