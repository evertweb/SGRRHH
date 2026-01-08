# Task Completion Checklist

## When adapting UI to Hospital/ForestechOil design:

### Code Quality
- [ ] No logic changes in C# code (unless absolutely necessary)
- [ ] All functionality preserved
- [ ] No new dependencies added

### Visual Consistency
- [ ] All inline styles removed (except dynamic/calculated)
- [ ] Using classes from `hospital.css`
- [ ] Using CSS variables where appropriate
- [ ] NO border-radius anywhere
- [ ] NO box-shadow (except focus states)
- [ ] NO animations or transitions
- [ ] Font is Courier New (inherited from hospital.css)

### Typography
- [ ] Page titles in UPPERCASE
- [ ] Labels in UPPERCASE (using `.required` for required fields)
- [ ] Table headers in UPPERCASE
- [ ] Button text in UPPERCASE

### Components
- [ ] Buttons use `.btn-*` classes
- [ ] Forms use `.form-group` or `.form-row`
- [ ] Tables have loading/empty/error states
- [ ] Modals use `FormModal` component
- [ ] Keyboard shortcuts visible where applicable

### States
- [ ] Loading state handled
- [ ] Empty state handled
- [ ] Error state handled
- [ ] Selected/active states use `.selected` class

### Before Committing
- [ ] Visual inspection matches other adapted pages
- [ ] No console errors
- [ ] Functionality tested
- [ ] Responsive behavior verified (if applicable)
