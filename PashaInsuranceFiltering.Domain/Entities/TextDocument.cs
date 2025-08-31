using PashaInsuranceFiltering.Domain.DomainEvents;
using PashaInsuranceFiltering.Domain.ValueObjects;
using PashaInsuranceFiltering.SharedKernel.Domain.Primitives.Abstract;
using PashaInsuranceFiltering.SharedKernel.Domain.Primitives.Guards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PashaInsuranceFiltering.Domain.Entities
{
    public sealed class TextDocument : AggregateRoot<Guid>
    {
        public string OriginalText { get; private set; } = string.Empty;
        public FilteredText? FilteredText { get; private set; }
        public bool IsProcessed { get; private set; }
        public SimilarityThreshold? Threshold { get; private set; }

        private TextDocument()
        {
            
        }

        public TextDocument(Guid uploadId, string originalText) : base(Guard.NotEmpty(uploadId, nameof(uploadId)))
        {
            OriginalText = Guard.NotNullOrWhiteSpace(originalText, nameof(originalText));
            IsProcessed = false;
            MarkAsCreated();
        }

        public void ApplyFiltering(string filteredText, double thresholdValue)
        {
            FilteredText = FilteredText.Create(filteredText);
            Threshold = SimilarityThreshold.Create(thresholdValue);
            IsProcessed = true;

            MarkAsModified();

            RaiseDomainEvent(new TextFilteredDomainEvent(Id));
        }

    }
}
