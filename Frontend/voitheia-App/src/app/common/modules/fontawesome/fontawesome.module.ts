import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FontAwesomeModule, FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { faPaperPlane, faUser, faPowerOff, faEdit, faTrashAlt, faKey } from '@fortawesome/free-solid-svg-icons';
import { faEyeSlash, faEye } from '@fortawesome/free-regular-svg-icons';



@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    FontAwesomeModule
  ],
  exports: [
    FontAwesomeModule
  ]
})
export class CustomFontawesomeModule {
  constructor(library: FaIconLibrary) {
    library.addIcons(faPaperPlane, faUser, faPowerOff, faEdit, faTrashAlt, faKey);
    library.addIcons(faEye, faEyeSlash);
  }
}
