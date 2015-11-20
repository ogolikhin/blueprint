// An example Backbone application contributed by
// [Jérôme Gravel-Niquet](http://jgn.me/). This demo uses a simple
// [LocalStorage adapter](backbone-localstorage.html)
// to persist Backbone models within your browser.

// Load the application once the DOM is ready, using `jQuery.ready`:
$(function(){

   'use strict';

  // Todo Model
  // ----------

  // Our basic **Todo** model has `title`, `order`, and `done` attributes.
  var Todo = Backbone.Model.extend({

    // Default attributes for the todo item.
    defaults: function() {
      return {
        title: 'a question', //TODO rename
        type: 'radio', //or checkbox
        order: survey.get('questions').nextOrder(),
        options: 'Yes, No, Not Sure',
        done: false //TODO - remove
      };
    },

    // Ensure that each todo created has `title`.
    initialize: function() {
      if (!this.get("title")) {
        this.set({"title": this.defaults().title});
      }
    },

  });

  // Todo Collection
  // ---------------

  // The collection of todos is backed by *localStorage* instead of a remote
  // server.
  var QuestionList = Backbone.Collection.extend({

    // Reference to this collection's model.
    model: Todo,

    // Save all of the todo items under the `"todos-backbone"` namespace.
    localStorage: new Backbone.LocalStorage("todos-backbone"),


    // We keep the Todos in sequential order, despite being saved by unordered
    // GUID in the database. This generates the next order number for new items.
    nextOrder: function() {
      if (!this.length) return 1;
      return this.last().get('order') + 1;
    },

    // Todos are sorted by their original insertion order.
    comparator: function(todo) {
      return todo.get('order');
    }
	
  });


  var Survey = Backbone.Model.extend({


    defaults: function() {
      return {
        title: 'a survey', //TODO rename
        questions: new QuestionList()
      };
    },

    // Ensure that each survey created has `title`.
    initialize: function() {
      if (!this.get("title")) {
        this.set({"title": this.defaults().title});
      }
      if (!this.get("questions")) {
        this.set({"questions": this.defaults().questions});
      }      
    },    

  });


  var survey = new Survey;

  // Todo Item View
  // --------------

  // The DOM element for a todo item...
  var QuestionView = Backbone.View.extend({

    //... is a list tag.
    tagName:  "li",

    // Cache the template function for a single item.
    template: _.template($('#item-template').html()),

    // The DOM events specific to an item.
    events: {
      "click a.destroy" : "clear",
      "keyup input.question"  : "updateQuestion",
      "keyup input.options"  : "updateOptions"
    },

    // The TodoView listens for changes to its model, re-rendering. Since there's
    // a one-to-one correspondence between a **Todo** and a **TodoView** in this
    // app, we set a direct reference on the model for convenience.
    initialize: function() {
      this.listenTo(this.model, 'change', this.render);
      this.listenTo(this.model, 'destroy', this.remove);
    },

    // Re-render the titles of the todo item.
    render: function() {
      this.$el.html(this.template(this.model.toJSON()));
      this.question = this.$('.question');
      this.options = this.$('.options');
      return this;
    },

    // Switch this view into `"editing"` mode, displaying the input field.
    edit: function() {
      this.$el.addClass("editing");
      this.input.focus();
    },

    // Close the `"editing"` mode, saving changes to the todo.
    close: function() {
      var value = this.input.val();
      if (!value) {
        this.clear();
      } else {
        this.model.save({title: value});
        this.$el.removeClass("editing");
      }
    },

    // If you hit `enter`, we're through editing the item.
    updateQuestion: function(e) {
      //if (e.keyCode == 13) this.close();
      var value = this.question.val();
      if (!value) {
        this.clear();
      } else {
        this.model.save({title: value});
      }      
    },

    updateOptions: function(e) {
      //if (e.keyCode == 13) this.close();
      var value = this.options.val();
      if (!value) {
        this.clear();
      } else {
        this.model.save({options: value});
      }      
    },    

    // Remove the item, destroy the model.
    clear: function() {
      this.model.destroy();
    }

  });

  // The Application
  // ---------------



  var Preview = Backbone.View.extend({

    // Instead of generating a new element, bind to the existing skeleton of
    // the App already present in the HTML.
    el: $("#preview"),

    titleTemplate: _.template($('#title-template').html()),


    // At initialization we bind to the relevant events on the `Todos`
    // collection, when items are added or changed. Kick things off by
    // loading any preexisting todos that might be saved in *localStorage*.
    initialize: function() {


      this.listenTo(survey.get('questions'), 'add', this.addOne);
      this.listenTo(survey.get('questions'), 'reset', this.addAll);
      this.listenTo(survey.get('questions'), 'all', this.render);


    this.titleSection = this.$('#title-section');

      survey.get('questions').fetch();

    },

    // Re-rendering the App just means refreshing the statistics -- the rest
    // of the app doesn't change.
    render: function() {


      if (survey.get('title').length) {
        this.titleSection.show();
        this.titleSection.html(this.titleTemplate({surveyTitle: survey.get('title')}));
      }

/*    
      if (survey.get('questions').length) {
        this.main.show();
        this.footer.show();
        this.footer.html(this.statsTemplate({done: done, remaining: remaining}));
      } else {
        this.main.hide();
        this.footer.hide();
      }
*/
    }


  });


  var preview = new Preview;

  // Our overall **AppView** is the top-level piece of UI.
  var ComposerView = Backbone.View.extend({

    // Instead of generating a new element, bind to the existing skeleton of
    // the App already present in the HTML.
    el: $("#composer"),

     // Delegated events for creating new items, and clearing completed ones.
    events: {
	    "keyup #set-title":  "setTitle",
      "click #add-checkbox": "addCheckboxQuestion",      
      "click #add-radio": "addRadioQuestion"
    },

    // At initialization we bind to the relevant events on the `Todos`
    // collection, when items are added or changed. Kick things off by
    // loading any preexisting todos that might be saved in *localStorage*.
    initialize: function() {

	    this.title = this.$("#set-title");
	  
      this.allCheckbox = this.$("#toggle-all")[0];

      this.listenTo(survey.get('questions'), 'add', this.addOne);
      this.listenTo(survey.get('questions'), 'reset', this.addAll);
      this.listenTo(survey.get('questions'), 'all', this.render);

      this.footer = this.$('footer');
      this.main = $('#main');

      survey.get('questions').fetch();
    },

    // Re-rendering the App just means refreshing the statistics -- the rest
    // of the app doesn't change.
    render: function() {
      var done = 0; //Todos.done().length;
      var remaining = 0; //Todos.remaining().length;
  
      if (survey.get('questions').length) {
        this.main.show();
      } else {
        this.main.hide();
      }

      //this.allCheckbox.checked = !remaining;
    },

    // Add a single todo item to the list by creating a view for it, and
    // appending its element to the `<ul>`.
    addOne: function(question) {
      var view = new QuestionView({model: question});
      this.$("#todo-list").append(view.render().el);
    },

    // Add all items in the **Todos** collection at once.
    addAll: function() {
      survey.get('questions').each(this.addOne, this);
    },

    addCheckboxQuestion: function() {
      survey.get('questions').create({title: 'Double click to customize checkbox question', type: 'checkbox'});
      this.updatePreview();      
    },

    addRadioQuestion: function() {
      survey.get('questions').create({title: 'Double click to customize radiobutton question', type: 'radio'});
      this.updatePreview();
    },    

    setTitle: function(e) {
      if (!this.title.val()) return;
      survey.set('title', this.title.val());
      this.updatePreview();
    },

    updatePreview: function() {
      preview.setElement(this.$('#preview')).render();      
    }


  });



  // Finally, we kick things off by creating the **App**.
  var composer = new ComposerView;


  $('#composer').append(composer.render());
  $('#preview').append(preview.render());

});
