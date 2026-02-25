// Favorite button heart animation
document.addEventListener('DOMContentLoaded', function() {
    document.addEventListener('submit', function(e) {
        const form = e.target;
        const favoriteBtn = form.querySelector('.favorite-btn');
        
        if (favoriteBtn && form.getAttribute('data-page') !== 'false') {
            e.preventDefault();
            
            const heartIcon = favoriteBtn.querySelector('.heart-icon');
            const isFavorite = favoriteBtn.dataset.isFavorite === 'true';
            
            if (!isFavorite) {
                // Heart pop animation when adding to favorites
                heartIcon.style.transition = 'transform 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275)';
                heartIcon.style.transform = 'scale(0)';
                
                setTimeout(() => {
                    heartIcon.classList.remove('fa-regular', 'text-base-content/50');
                    heartIcon.classList.add('fa-solid', 'text-error');
                    favoriteBtn.dataset.isFavorite = 'true';
                    
                    const hiddenInput = document.createElement('input');
                    hiddenInput.type = 'hidden';
                    hiddenInput.name = 'favoritoId';
                    hiddenInput.value = '';
                    form.appendChild(hiddenInput);
                    
                    heartIcon.style.transform = 'scale(1.5)';
                    
                    setTimeout(() => {
                        heartIcon.style.transform = 'scale(1)';
                        form.submit();
                    }, 200);
                }, 150);
            } else {
                // Shrink animation when removing from favorites
                heartIcon.style.transition = 'transform 0.2s ease-in';
                heartIcon.style.transform = 'scale(0)';
                
                setTimeout(() => {
                    heartIcon.classList.remove('fa-solid', 'text-error');
                    heartIcon.classList.add('fa-regular', 'text-base-content/50');
                    favoriteBtn.dataset.isFavorite = 'false';
                    
                    // Remove the hidden input for favoritoId
                    const favoritoInput = form.querySelector('input[name="favoritoId"]');
                    if (favoritoInput) {
                        favoritoInput.remove();
                    }
                    
                    heartIcon.style.transform = 'scale(0.5)';
                    
                    setTimeout(() => {
                        heartIcon.style.transform = 'scale(1)';
                        form.submit();
                    }, 150);
                }, 200);
            }
        }
    });
});

// Product card hover effects - add smooth transitions
document.addEventListener('DOMContentLoaded', function() {
    const cards = document.querySelectorAll('.card');
    
    cards.forEach(card => {
        card.style.transition = 'transform 0.4s ease, box-shadow 0.4s ease, border-color 0.4s ease';
        
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-8px)';
            this.style.boxShadow = '0 20px 40px -12px rgba(0, 0, 0, 0.15)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
            this.style.boxShadow = '';
        });
    });
});

// Image zoom on hover
document.addEventListener('DOMContentLoaded', function() {
    const images = document.querySelectorAll('.img-zoom');
    
    images.forEach(img => {
        const container = img.closest('figure');
        if (container) {
            container.style.overflow = 'hidden';
            img.style.transition = 'transform 0.5s ease';
            
            container.addEventListener('mouseenter', function() {
                img.style.transform = 'scale(1.08)';
            });
            
            container.addEventListener('mouseleave', function() {
                img.style.transform = 'scale(1)';
            });
        }
    });
});
