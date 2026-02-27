// Favorite button heart animation - improved version
document.addEventListener('DOMContentLoaded', function() {
    document.addEventListener('submit', function(e) {
        const form = e.target;
        const favoriteBtn = form.querySelector('.favorite-btn');
        
        if (favoriteBtn && form.getAttribute('data-page') !== 'false') {
            e.preventDefault();
            
            const heartIcon = favoriteBtn.querySelector('.heart-icon');
            const isFavorite = favoriteBtn.dataset.isFavorite === 'true';
            
            // Create burst effect element
            const createBurst = () => {
                const burst = document.createElement('span');
                burst.style.cssText = `
                    position: absolute;
                    width: 100%;
                    height: 100%;
                    border-radius: 50%;
                    background: radial-gradient(circle, rgba(239,68,68,0.4) 0%, transparent 70%);
                    animation: burstAnim 0.5s ease-out forwards;
                    pointer-events: none;
                `;
                favoriteBtn.style.position = 'relative';
                favoriteBtn.appendChild(burst);
                setTimeout(() => burst.remove(), 500);
            };
            
            if (!isFavorite) {
                // Dramatic heart pop animation when adding to favorites
                heartIcon.style.transition = 'transform 0.15s ease-out';
                heartIcon.style.transform = 'scale(0)';
                
                // Button scale animation
                favoriteBtn.style.transition = 'transform 0.15s ease-out';
                favoriteBtn.style.transform = 'scale(0.85)';
                
                createBurst();
                
                setTimeout(() => {
                    // Change to solid red heart
                    heartIcon.classList.remove('fa-regular', 'text-base-content/50');
                    heartIcon.classList.add('fa-solid', 'text-error');
                    favoriteBtn.dataset.isFavorite = 'true';
                    
                    const hiddenInput = document.createElement('input');
                    hiddenInput.type = 'hidden';
                    hiddenInput.name = 'favoritoId';
                    hiddenInput.value = '';
                    form.appendChild(hiddenInput);
                    
                    // Elastic bounce effect
                    heartIcon.style.transition = 'transform 0.4s cubic-bezier(0.68, -0.55, 0.265, 1.55)';
                    heartIcon.style.transform = 'scale(1.4)';
                    
                    favoriteBtn.style.transform = 'scale(1.15)';
                    
                    setTimeout(() => {
                        heartIcon.style.transform = 'scale(1)';
                        favoriteBtn.style.transform = 'scale(1)';
                        
                        setTimeout(() => {
                            form.submit();
                        }, 100);
                    }, 250);
                }, 120);
            } else {
                // Dramatic shrink animation when removing from favorites
                heartIcon.style.transition = 'transform 0.2s ease-in';
                heartIcon.style.transform = 'scale(0) rotate(-30deg)';
                
                favoriteBtn.style.transition = 'transform 0.2s ease-in';
                favoriteBtn.style.transform = 'scale(0.9)';
                
                setTimeout(() => {
                    heartIcon.classList.remove('fa-solid', 'text-error');
                    heartIcon.classList.add('fa-regular', 'text-base-content/50');
                    favoriteBtn.dataset.isFavorite = 'false';
                    
                    const favoritoInput = form.querySelector('input[name="favoritoId"]');
                    if (favoritoInput) {
                        favoritoInput.remove();
                    }
                    
                    heartIcon.style.transition = 'transform 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275)';
                    heartIcon.style.transform = 'scale(0.6) rotate(0deg)';
                    
                    favoriteBtn.style.transform = 'scale(1.1)';
                    
                    setTimeout(() => {
                        heartIcon.style.transform = 'scale(1)';
                        favoriteBtn.style.transform = 'scale(1)';
                        
                        setTimeout(() => {
                            form.submit();
                        }, 100);
                    }, 200);
                }, 200);
            }
        }
    });
});

// Add burst animation keyframes
const style = document.createElement('style');
style.textContent = `
    @keyframes burstAnim {
        0% {
            transform: scale(0.5);
            opacity: 1;
        }
        100% {
            transform: scale(2.5);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);

// Product card hover effects
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
