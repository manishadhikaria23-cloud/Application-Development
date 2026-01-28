using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using JournalApp.Models.Entities;
using JournalApp.Services.Interfaces;

namespace JournalApp.Components.Pages.Journal
{
    public partial class Today : ComponentBase
    {
        [Inject] public IJournalEntryService JournalService { get; set; } = default!;
        [Inject] public IJSRuntime JS { get; set; } = default!;
        [Inject] public NavigationManager Nav { get; set; } = default!;

        protected JournalEntry Model { get; set; } = new JournalEntry();
        protected HashSet<string> SecondaryMoodSet { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        protected bool HasEntry { get; set; }
        protected bool IsSaving { get; set; }
        protected string Message { get; set; } = string.Empty;
        protected DateTime? LastSaved { get; set; }

        // editor state
        protected ElementReference EditorRef;
        protected string editorPlainText = string.Empty;

        // ensure we initialize the JS editor once after render
        private bool _editorInitialized;

        protected bool CanSave =>
            !string.IsNullOrWhiteSpace(Model?.Title) &&
            !string.IsNullOrWhiteSpace(editorPlainText) &&
            !IsSaving;

        protected override async Task OnInitializedAsync()
        {
            await LoadAsync();
        }

        // Load today's entry or initialize new (no JS calls here)
        protected async Task LoadAsync()
        {
            Message = string.Empty;
            IsSaving = false;

            try
            {
                var today = await JournalService.GetTodayEntryAsync();
                if (today != null)
                {
                    Model = today;
                    HasEntry = true;
                    SecondaryMoodSet.Clear();

                    if (!string.IsNullOrWhiteSpace(Model.SecondaryMoods))
                    {
                        foreach (var m in Model.SecondaryMoods
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .Where(s => !string.IsNullOrWhiteSpace(s)))
                        {
                            SecondaryMoodSet.Add(m);
                        }
                    }
                }
                else
                {
                    Model = new JournalEntry { EntryDate = DateTime.Today };
                    HasEntry = false;
                    SecondaryMoodSet.Clear();
                }
            }
            catch (Exception ex)
            {
                Message = "Failed to load today's entry.";
                Console.WriteLine(ex);
            }
        }

        // Initialize JS editor after component is rendered and DOM is ready
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !_editorInitialized)
            {
                try
                {
                    // set initial HTML (may be null)
                    await JS.InvokeVoidAsync("richEditor.setHtml", Model.Content ?? string.Empty);

                    // read back plain text to update word count and CanSave
                    editorPlainText = await JS.InvokeAsync<string>("richEditor.getText");
                }
                catch (JSException jsEx)
                {
                    Console.WriteLine("richEditor init error: " + jsEx);
                    Message = "Rich editor failed to initialize (check console).";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    _editorInitialized = true;
                    StateHasChanged();
                }
            }
        }

        // Execute browser command (bold/italic/list/etc.)
        protected async Task ExecCommand(string command, string? value = null)
        {
            try
            {
                await JS.InvokeVoidAsync("richEditor.exec", command, value);
                // update internal text/cache after command
                editorPlainText = await JS.InvokeAsync<string>("richEditor.getText");
                Model.Content = await JS.InvokeAsync<string>("richEditor.getHtml");
                StateHasChanged();
            }
            catch (JSException jsEx)
            {
                Console.WriteLine("ExecCommand JS error: " + jsEx);
            }
        }

        // Editor input handler (updates Model.Content and plain text)
        protected async Task OnEditorInput()
        {
            try
            {
                Model.Content = await JS.InvokeAsync<string>("richEditor.getHtml");
                editorPlainText = await JS.InvokeAsync<string>("richEditor.getText");
                StateHasChanged();
            }
            catch (JSException jsEx)
            {
                Console.WriteLine("OnEditorInput JS error: " + jsEx);
            }
        }

        // Insert snippet (HTML) programmatically (used for quick actions)
        protected async Task InsertHtmlSnippet(string htmlSnippet)
        {
            try
            {
                await JS.InvokeVoidAsync("richEditor.exec", "insertHTML", htmlSnippet);
                editorPlainText = await JS.InvokeAsync<string>("richEditor.getText");
                Model.Content = await JS.InvokeAsync<string>("richEditor.getHtml");
                StateHasChanged();
            }
            catch (JSException jsEx)
            {
                Console.WriteLine("InsertHtmlSnippet JS error: " + jsEx);
            }
        }

        // Save or update today's entry (stores HTML in Model.Content)
        protected async Task SaveAsync()
        {
            Message = string.Empty;
            IsSaving = true;

            try
            {
                // refresh content before validating
                Model.Content = await JS.InvokeAsync<string>("richEditor.getHtml");
                editorPlainText = await JS.InvokeAsync<string>("richEditor.getText");
            }
            catch (JSException jsEx)
            {
                Console.WriteLine("Save: reading editor failed: " + jsEx);
            }

            if (string.IsNullOrWhiteSpace(Model.Title))
            {
                Message = "Title is required.";
                IsSaving = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(editorPlainText))
            {
                Message = "Content cannot be empty.";
                IsSaving = false;
                return;
            }

            if (SecondaryMoodSet.Count > 2)
            {
                Message = "You can select up to 2 secondary moods only.";
                IsSaving = false;
                return;
            }    

            Model.SecondaryMoods = string.Join(", ", SecondaryMoodSet);

            try
            {
                var saved = await JournalService.CreateOrUpdateTodayAsync(Model);
                if (saved != null)
                {
                    Model = saved;
                    HasEntry = true;
                    Message = "Saved successfully.";
                    LastSaved = DateTime.Now;
                    // ensure editor HTML matches saved content (in case server-normalization)
                    await JS.InvokeVoidAsync("richEditor.setHtml", Model.Content ?? string.Empty);
                }
                else
                {
                    Message = "Save completed but no entry returned.";
                }
            }
            catch (Exception ex)
            {
                Message = "Something went wrong while saving. Please try again.";
                Console.WriteLine(ex);
            }
            finally
            {
                IsSaving = false;
                StateHasChanged();
            }
        }

        // Delete today's entry
        protected async Task DeleteAsync()
        {
            Message = string.Empty;

            try
            {
                var ok = await JournalService.DeleteTodayAsync();
                if (ok)
                {
                    Model = new JournalEntry { EntryDate = DateTime.Today };
                    HasEntry = false;
                    SecondaryMoodSet.Clear();
                    Message = "Deleted successfully.";
                    LastSaved = null;
                    await JS.InvokeVoidAsync("richEditor.setHtml", string.Empty);
                    editorPlainText = string.Empty;
                }
                else
                {
                    Message = "No entry found to delete.";
                }
            }
            catch (Exception ex)
            {
                Message = "Something went wrong while deleting. Please try again.";
                Console.WriteLine(ex);
            }
            finally
            {
                StateHasChanged();
            }
        }

        protected int CountWords(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            return text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        protected void OpenTodayView()
        {
            Nav.NavigateTo($"/view/{Model.EntryDate:yyyy-MM-dd}");
        }

        // Added: handle checkbox change for secondary moods without changing existing save/load logic
        protected void OnSecondaryMoodChanged(string mood, ChangeEventArgs e)
        {
            try
            {
                var isChecked = e?.Value is bool b && b;

                if (isChecked)
                {
                    // enforce maximum of 2 secondary moods
                    if (!SecondaryMoodSet.Contains(mood) && SecondaryMoodSet.Count >= 2)
                    {
                        Message = "You can select up to 2 secondary moods only.";
                        return;
                    }

                    SecondaryMoodSet.Add(mood);
                }
                else
                {
                    SecondaryMoodSet.Remove(mood);
                }

                // clear any previous message on valid change
                Message = string.Empty;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnSecondaryMoodChanged error: " + ex);
            }
        }
    }
}