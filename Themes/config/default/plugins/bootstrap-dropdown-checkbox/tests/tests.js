function setup(options) {
  var $fixture = $('#qunit-fixture'),
      $div = $('<div/>');

  $(document).off();
  $fixture.off();
  options = $.extend({}, { data: [], hideHeader: false }, options || {});
  $fixture.append($div);
  $div.dropdownCheckbox(options);
  return $div.data('dropdownCheckbox');
}

function list(size, checked) {
  var result = [];
  for (var i = 0; i < size; i++) {
    result.push({
      id: i,
      label: 'Item #' + i,
      isChecked: checked === undefined ? !!(Math.round(Math.random() * 1)) : checked
    });
  }
  return result;
}

module('Constructor');
test('widget can be constructed using the jquery plugin', function() {
  var widget = setup({
    data: [{ id: 1, label: 'yes', isChecked: false }]
  });
  ok(widget, 'widget was constructed');
});

test('widget will not create itself twice if initialized on the same element and ignores the configuration', function() {
  var widget = setup({
    data: [{ id: 1, label: 'yes', isChecked: false }]
  });
  $('#qunit-fixture > div').dropdownCheckbox({ data: [] });
  var newWidget = $('#qunit-fixture > div').data('dropdownCheckbox');
  equal(widget, newWidget, 'No new widget was created');
  equal(widget.data.length, 1, 'New widget configuration was not applied');
});


module('Methods');
test('#items returns the entire list of data items', function() {
  var items = list(25);
  var widget = setup({
    data: items
  });
  equal(widget.items().length, 25, 'Items is the right length');
});

test('#checked returns the list of checked data items', function() {
  var items = list(25, false);
  for (var i = 0; i < 5; i++) {
    items[i].isChecked = true;
  }
  var widget = setup({
    data: items
  });

  equal(widget.checked().length, 5, 'Items is the right length');
  for (i = 0; i < 5; i++) {
    deepEqual(widget.data[i], items[i], 'The right items are checked');
  }
});

test('#unchecked returns the list of unchecked data items', function() {
  var items = list(10, true);
  for (var i = 0; i < 5; i++) {
    items[i].isChecked = false;
  }
  var widget = setup({
    data: items
  });

  equal(widget.unchecked().length, 5, 'Unchecked returns the right number of items');
  for (i = 0; i < 5; i++) {
    deepEqual(widget.data[i], items[i], 'The right items are unchecked');
  }
});

test('#append appends a single new data item and renders the corresponding element', function() {
  var widget = setup();

  widget.append({ id: 1, label: 'single item', isChecked: true });
  equal(widget.data.length, 1, 'Single item');
  equal(widget.$list.find('li').length, 1, 'One item found in the list');
});

test('#append appends n data items from an array and renders the corresponding elements', function() {
  var widget = setup();
  var data = list(25);

  widget.append(data);
  equal(widget.data.length, 25, '25 data items found');
  equal(widget.$list.find('li').length, 25, '25 items found in the list');
});

test('#remove removes one data item and its corresponding elements', function() {
  var data = list(5);
  var removedItem = data[2];
  var widget = setup({ data: data });

  widget.remove(removedItem.id);
  equal(widget.data.length, 4, '4 data items found');
  equal(widget.$list.find('li').length, 4, '4 items found in the list');
  equal(widget.data.indexOf(removedItem), -1, 'item has been removed from the list');
});

test('#remove removes multiple data items and corresponding elements', function() {
  var data = list(5);
  var removedItems = [data[2], data[3]];
  var widget = setup({ data: data });

  widget.remove([removedItems[0].id, removedItems[1].id]);
  equal(widget.data.length, 3, '3 data items found');
  equal(widget.$list.find('li').length, 3, '3 items found in the list');
  equal(widget.data.indexOf(removedItems[0]), -1, 'First item has been removed from the list');
  equal(widget.data.indexOf(removedItems[1]), -1, 'Second item has been removed from the list');
});

test('#reset resets the dropdown with a new list of items', function() {
  var initialData = list(10);
  var resetData = list(5);
  var widget = setup({ data: initialData });

  equal(widget.data.length, 10, '10 data items found after initial setup');
  equal(widget.$list.find('li').length, 10, '10 items found in the list');

  widget.reset(resetData);

  equal(widget.data.length, 5, '5 data items found after initial setup');
  equal(widget.$list.find('li').length, 5, '5 items found in the list');
});

module('Events');
test('change:dropdown-checkbox is triggered with there are changes and the dropdown is closed', function() {
  expect(2);

  var initialData = list(10);
  var widget = setup({ data: initialData });

  $('#qunit-fixture').on('change:dropdown-checkbox', function() {
    ok(true, 'Event handler was called');
  });

  widget.$element.click();
  widget.$list.find('li input[type=checkbox]').last().click();
  widget.$element.click();

  ok(widget.$element.hasClass('open') === false, 'Dropdown is closed');
});

test('change:dropdown-checkbox is not triggered when there are no changes and the dropdown is closed', function() {
  expect(1);

  var initialData = list(10);
  var widget = setup({ data: initialData });

  $('#qunit-fixture').on('change:dropdown-checkbox', function() {
    ok(true, 'Event handler was called');
  });

  widget.$element.click();
  widget.$element.click();

  ok(widget.$element.hasClass('open') === false, 'Dropdown is closed');
});

test('change:dropdown-checkbox is not triggered when there are no changes and the dropdown is closed', function() {
  expect(1);

  var initialData = list(10);
  var widget = setup({ data: initialData });

  $('#qunit-fixture').on('change:dropdown-checkbox', function() {
    ok(true, 'Event handler was called');
  });

  widget.$element.click();
  widget.$element.click();

  ok(widget.$element.hasClass('open') === false, 'Dropdown is closed');
});


test('checked:all and check:all are triggered with true when it is checked and all other checkboxes are unchecked', function() {
  expect(4);

  var initialData = list(10, false);
  var widget = setup({ data: initialData });

  equal(widget.$parent.find('input[type=checkbox]:checked').length, 0, 'No checkboxes found checked');

  $('#qunit-fixture').on('checked:all', function(e, checked) {
    ok(checked === true, 'Event handler was called');
  });

  $('#qunit-fixture').on('check:all', function() {
    ok(true, 'check:all triggered');
  });

  widget.$element.click();
  widget.$parent.find('.checkbox-all').click();
  widget.$element.click();

  equal(widget.$list.find('input[type=checkbox]:checked').length, 10, 'All checkbox found checked');
});

test('checked:all and uncheck:all is triggered with false when it is unchecked and all other checkboxes are unchecked', function() {
  expect(4);

  var initialData = list(10, true);
  var widget = setup({ data: initialData });

  ok(widget.$parent.find('.checkbox-all')[0].checked, 'Select all is already checked, because all the items are checked');

  $('#qunit-fixture').on('checked:all', function(e, checked) {
    ok(checked === false, 'Event handler was called');
  });
  $('#qunit-fixture').on('uncheck:all', function() {
    ok(true, 'uncheck:all triggered');
  });

  widget.$element.click();
  widget.$parent.find('.checkbox-all').click();
  widget.$element.click();

  equal(widget.$list.find('input[type=checkbox]:checked').length, 0, 'No checkboxes found checked');
});

test('check:checkbox and "checked" is triggered when a single checkbox is checked', function(){
  expect(2);

  var initialData = list(10, false);
  var widget = setup({ data: initialData });

  $('#qunit-fixture').on('check:checkbox', function() {
    ok(true, 'Event handler was called');
  });

  $('#qunit-fixture').on('checked', function(e, value) {
    equal(value, true, 'Checked event handler was called with true');
  });

  widget.$element.click();
  widget.$list.find('li input[type=checkbox]').last().click();
});

test('uncheck:checkbox and "checked" is triggered when a single checkbox is unchecked', function(){
  expect(2);

  var initialData = list(10, true);
  var widget = setup({ data: initialData });

  $('#qunit-fixture').on('uncheck:checkbox', function() {
    ok(true, 'Event handler was called');
  });

  $('#qunit-fixture').on('checked', function(e, value) {
    equal(value, false, 'Checked event handler was called with false');
  });

  widget.$element.click();
  widget.$list.find('li input[type=checkbox]').last().click();
});

module('Alternate AKA stupid mode');
test('when initializing with all checked items, the checkall checkbox is checked, all models are checked, but none of the checkboxes are', function(){
  expect(3);

  var initialData = list(10, true);
  var widget = setup({ data: initialData, alternate: true });

  equal(widget.$parent.find('.checkbox-all:checked').length, 1, 'All checked checkbox is checked');
  equal(widget.$list.find('input[type=checkbox]:checked').length, 0, 'No checkboxes found checked');
  equal(widget._allChecked(), true, 'All data is checked data');
});

test('when initializing with all checked items, the checkall checkbox is checked, all models are checked, but none of the checkboxes are', function(){
  expect(3);

  var initialData = list(10, true);
  var widget = setup({ data: initialData, alternate: true });

  equal(widget.$parent.find('.checkbox-all:checked').length, 1, 'All checked checkbox is checked');
  equal(widget.$list.find('input[type=checkbox]:checked').length, 0, 'No checkboxes found checked');
  equal(widget._allChecked(), true, 'All data is checked');
});

test('when initializing with all checked items, if i select one checkbox, one model is checked, the all checked is not checked', function(){
  expect(5);

  var initialData = list(10, true);
  var widget = setup({ data: initialData, alternate: true });

  equal(widget.$list.find('input[type=checkbox]:checked').length, 0, 'No checkboxes found checked');

  widget.$list.find('input[type=checkbox]').first().click();

  equal(widget.$parent.find('.checkbox-all:checked').length, 0, 'All checked checkbox is not checked');
  equal(widget.$list.find('input[type=checkbox]:checked').length, 1, 'One checkboxes found checked');
  equal(widget._checkedLength(), 1, '1 data item is checked');
  deepEqual(widget.checked(), [initialData[0]], 'The right item is checked');
});

test('when initializing with all unchecked items, the checkall checkbox is checked, all models are checked and none of the checkboxes are checked', function(){
  expect(3);

  var initialData = list(10, false);
  var widget = setup({ data: initialData, alternate: true });

  equal(widget.$parent.find('.checkbox-all:checked').length, 1, 'All checked checkbox is checked');
  equal(widget.$list.find('input[type=checkbox]:checked').length, 0, 'No checkboxes found checked');
  equal(widget._allChecked(), true, 'All data is checked');
});

test('when initializing with mixed list of items, the checkall checkbox is unchecked, some models are checked and some of the checkboxes are checked', function(){
  expect(3);

  var initialData = list(10, false);
  initialData[0].isChecked = true;
  initialData[3].isChecked = true;
  initialData[7].isChecked = true;

  var widget = setup({ data: initialData, alternate: true });

  equal(widget.$parent.find('.checkbox-all:checked').length, 0, 'Checkall checkbox is not checked');
  equal(widget.$list.find('input[type=checkbox]:checked').length, 3, '3 checkboxes found checked');
  deepEqual(widget.checked(), [initialData[0], initialData[3], initialData[7]], 'The right data items are checked');
});

test('when initializing with mixed list of items, the checkall checkbox is unchecked, some models are checked and some of the checkboxes are checked', function(){
  expect(3);

  var initialData = list(10, false);
  initialData[0].isChecked = true;
  initialData[3].isChecked = true;
  initialData[7].isChecked = true;

  var widget = setup({ data: initialData, alternate: true });

  equal(widget.$parent.find('.checkbox-all:checked').length, 0, 'Checkall checkbox is not checked');
  equal(widget.$list.find('input[type=checkbox]:checked').length, 3, '3 checkboxes found checked');
  deepEqual(widget.checked(), [initialData[0], initialData[3], initialData[7]], 'The right data items are checked');
});

test('whhen there is a maxitems set and the checkall checkbox is checked, all models are checked', function(){
  expect(3);

  var initialData = list(100, false);
  initialData[0].isChecked = true;
  initialData[3].isChecked = true;
  initialData[7].isChecked = true;

  var widget = setup({ data: initialData, alternate: true, maxItems: 10 });
  widget.$parent.find('.checkbox-all').click();

  equal(widget.$parent.find('.checkbox-all:checked').length, 1, 'Checkall checkbox is checked');
  equal(widget.$list.find('input[type=checkbox]:checked').length, 0, 'No checkboxes found checked');
  equal(widget.checked().length, 100, 'All the items are checked');
});







